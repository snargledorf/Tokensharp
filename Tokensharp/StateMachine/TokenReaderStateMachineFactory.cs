using SwiftState;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal static class TokenReaderStateMachineFactory<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static TokenReaderStateMachine<TTokenType> BuildStateMachine(ITokenConfiguration<TTokenType> tokenConfiguration)
    {
        TokenTree<TTokenType> tree = tokenConfiguration.ToTokenTree();
            
        var startStateBuilder = new StateBuilder<char, TokenizerStateId<TTokenType>>(TokenizerStateId<TTokenType>.Start);
            
        BuildTreeTransitions(startStateBuilder, tree);

        BuildTextWhiteSpaceAndNumberTransitions(startStateBuilder, tree);

        return new TokenReaderStateMachine<TTokenType>(startStateBuilder.Build());
    }

    private static void BuildTextWhiteSpaceAndNumberTransitions(
        IStateBuilder<char, TokenizerStateId<TTokenType>> startStateBuilder,
        TokenTree<TTokenType> tree)
        {
            IStateBuilder<char, TokenizerStateId<TTokenType>> whiteSpaceStateBuilder = startStateBuilder.When(c => char.IsWhiteSpace(c), TokenizerStateId<TTokenType>.WhiteSpace);
            IStateBuilder<char, TokenizerStateId<TTokenType>> numberStateBuilder = startStateBuilder.When(c => char.IsDigit(c), TokenizerStateId<TTokenType>.Number);
            IStateBuilder<char, TokenizerStateId<TTokenType>> textStateBuilder = startStateBuilder.Default(TokenizerStateId<TTokenType>.Text);

            whiteSpaceStateBuilder.When(c => !char.IsWhiteSpace(c), TokenizerStateId<TTokenType>.EndOfWhiteSpace, true);
            numberStateBuilder.When(c => !char.IsDigit(c), TokenizerStateId<TTokenType>.EndOfNumber, true);
            textStateBuilder.When(c => char.IsDigit(c) || char.IsWhiteSpace(c), TokenizerStateId<TTokenType>.EndOfText, true);

            foreach (TokenTreeNode<TTokenType> node in tree)
            {
                TokenizerStateId<TTokenType> fallbackStateId;
                if (node.IsWhiteSpaceToRoot)
                {
                    fallbackStateId = TokenizerStateId<TTokenType>.WhiteSpace;
                }
                else if (node.IsDigitsToRoot)
                {
                    fallbackStateId = TokenizerStateId<TTokenType>.Number;
                }
                else
                {
                    fallbackStateId = TokenizerStateId<TTokenType>.Text;
                }
                
                IStateBuilder<char, TokenizerStateId<TTokenType>> fallbackStateBuilder = startStateBuilder.GetBuilderForState(fallbackStateId);

                if (node.IsEndOfToken)
                {
                    TokenizerStateId<TTokenType> terminalStateId =
                        TokenizerStateId<TTokenType>.CreateTerminal(fallbackStateId.TokenType);

                    fallbackStateBuilder.When(node.Key, terminalStateId, true);
                }
                
                BuildNodeTransitions(node, fallbackStateBuilder, fallbackStateId);
            }
        }

        private static void BuildTreeTransitions(
            IStateBuilder<char, TokenizerStateId<TTokenType>> builder,
            TokenTree<TTokenType> tree)
        {
            foreach (TokenTreeNode<TTokenType> node in tree)
            {
                TokenizerStateId<TTokenType> fallbackStateId = GetFallBackStateId(node);
                BuildNodeTransitions(node, builder, fallbackStateId);
            }
        }

        private static void BuildNodeTransitions(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> currentTokenizerStateBuilder,
            TokenizerStateId<TTokenType> fallbackStateId)
        {
            if (node.HasChildren)
            {
                BuildTransitionsForNodeWithChildren(node, currentTokenizerStateBuilder, fallbackStateId);
            }
            else
            {
                BuildTransitionsForNodeWithNoChildren(node, currentTokenizerStateBuilder);
            }
        }

        private static void BuildTransitionsForNodeWithChildren(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> previousNodeStateBuilder,
            TokenizerStateId<TTokenType> fallbackStateId)
        {
            if (!node.HasChildren)
                throw new InvalidOperationException("node has no children");

            // Build follow-up states to check for longer control strings
            //-------------------------------------------------------------------------------
            // Ex 2. <Foo vs. <FooB vs. <FooBar

            IStateBuilder<char, TokenizerStateId<TTokenType>> currentNodeStateBuilder = previousNodeStateBuilder.When(node.Key, node.StateId);

            if (node.IsEndOfToken)
            {
                TokenizerStateId<TTokenType> terminalStateId =
                    TokenizerStateId<TTokenType>.CreateTerminal(node.StateId.TokenType);
                
                currentNodeStateBuilder.Default(terminalStateId, true);

                fallbackStateId = terminalStateId;
            }
            else
            {
                // Add a default in case we don't match on the nodes key
                currentNodeStateBuilder.Default(fallbackStateId, fallbackStateId.IsTerminal);
            }

            foreach (TokenTreeNode<TTokenType> childNode in node)
            {
                BuildNodeTransitions(childNode, currentNodeStateBuilder, fallbackStateId);
            }
        }

        private static void BuildTransitionsForNodeWithNoChildren(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> previousNodeStateBuilder)
        {
            if (node.HasChildren)
                throw new InvalidOperationException("node has children");
            if (!node.IsEndOfToken)
                throw new InvalidOperationException("node is not an end of token");
            
            // If this node had no children, then we need to switch to a dummy state
            // to ensure the character is read
            IStateBuilder<char, TokenizerStateId<TTokenType>> currentNodeStateBuilder =
                previousNodeStateBuilder.When(node.Key, node.StateId);

            // Default to terminal state for the final node
            TokenizerStateId<TTokenType> terminalStateId =
                TokenizerStateId<TTokenType>.CreateTerminal(node.StateId.TokenType);
            
            currentNodeStateBuilder.Default(terminalStateId, true);

            // If this whole branch is whitespace, then add checks for WhiteSpace situations
            if (node.IsWhiteSpaceToRoot)
            {
                TokenTreeNode<TTokenType> rootNode = node.RootNode;

                // We might be starting another one of these branches \r\r\n = \r {whitespace} \r\n {record}
                previousNodeStateBuilder.When(rootNode.Key, TokenizerStateId<TTokenType>.EndOfWhiteSpace, true);

                // Or there could be some other type of whitespace character \r\t = {whitespace}
                previousNodeStateBuilder.When(c => char.IsWhiteSpace(c), TokenizerStateId<TTokenType>.WhiteSpace);
            }
        }

        private static TokenizerStateId<TTokenType> GetFallBackStateId(TokenTreeNode<TTokenType> node)
        {
            if (node.IsWhiteSpaceToRoot)
                return TokenizerStateId<TTokenType>.WhiteSpace;
            if (node.IsDigitsToRoot)
                return TokenizerStateId<TTokenType>.Number;
            
            return TokenizerStateId<TTokenType>.Text;
        }
}
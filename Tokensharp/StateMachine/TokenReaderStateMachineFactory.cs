using SwiftState;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal static class TokenReaderStateMachineFactory<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static TokenReaderStateMachine<TTokenType> BuildStateMachine(TokenConfiguration<TTokenType> tokenConfiguration)
    {
        TokenTree<TTokenType> tree = tokenConfiguration.ToTokenTree();
            
        var startStateBuilder = new StateBuilder<char, TokenizerStateId<TTokenType>>(TokenizerStateId<TTokenType>.Start);

        var context = new TokenizerStateMachineContext(tree);
            
        BuildTreeTransitions(startStateBuilder, context);

        BuildTextWhiteSpaceAndNumberTransitions(startStateBuilder, context);

        return new TokenReaderStateMachine<TTokenType>(startStateBuilder.Build());
    }

    private static void BuildTextWhiteSpaceAndNumberTransitions(
            StateBuilder<char, TokenizerStateId<TTokenType>> startStateBuilder, 
            TokenizerStateMachineContext context)
        {
            startStateBuilder.When(c => char.IsWhiteSpace(c), context.WhiteSpaceStateBuilder);
            startStateBuilder.When(c => char.IsDigit(c), context.NumberStateBuilder);
            startStateBuilder.Default(context.TextStateBuilder);

            context.WhiteSpaceStateBuilder.When(c => !char.IsWhiteSpace(c), context.EndOfWhiteSpaceStateBuilder);
            context.NumberStateBuilder.When(c => !char.IsDigit(c), context.EndOfNumberStateBuilder);
            context.TextStateBuilder.When(c => char.IsDigit(c) || char.IsWhiteSpace(c), context.EndOfTextStateBuilder);

            foreach (TokenTreeNode<TTokenType> node in context.Tree)
            {
                BuildNodeTransitions(node, context.WhiteSpaceStateBuilder, context,
                    TokenizerStateId<TTokenType>.WhiteSpace);
                BuildNodeTransitions(node, context.NumberStateBuilder, context, TokenizerStateId<TTokenType>.Number);
                BuildNodeTransitions(node, context.TextStateBuilder, context, TokenizerStateId<TTokenType>.Text);
            }
        }

        private static void BuildTreeTransitions(IStateBuilder<char, TokenizerStateId<TTokenType>> builder, TokenizerStateMachineContext context)
        {
            foreach (TokenTreeNode<TTokenType> node in context.Tree)
            {
                TokenizerStateId<TTokenType> fallbackStateId = GetFallBackStateId(node);
                BuildNodeTransitions(node, builder, context, fallbackStateId);
            }
        }

        private static void BuildNodeTransitions(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> currentTokenizerStateBuilder,
            TokenizerStateMachineContext context,
            TokenizerStateId<TTokenType> fallbackStateId)
        {
            if (node.HasChildren)
            {
                BuildTransitionsForNodeWithChildren(node, currentTokenizerStateBuilder, context, fallbackStateId);
            }
            else
            {
                BuildTransitionsForNodeWithNoChildren(node, currentTokenizerStateBuilder, context);
            }
        }

        private static void BuildTransitionsForNodeWithChildren(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> previousNodeStateBuilder,
            TokenizerStateMachineContext context,
            TokenizerStateId<TTokenType> fallbackStateId)
        {
            if (!node.HasChildren)
                throw new InvalidOperationException("node has no children");

            // Build follow-up states to check for longer control strings
            //-------------------------------------------------------------------------------
            // Ex 2. <Foo vs. <FooB vs. <FooBar

            IStateBuilder<char, TokenizerStateId<TTokenType>> currentNodeStateBuilder = context.GetStateBuilder(node.StateId);

            previousNodeStateBuilder.When(node.Key, currentNodeStateBuilder);

            if (node.IsEndOfToken)
            {
                TokenizerStateId<TTokenType> terminalStateId =
                    TokenizerStateId<TTokenType>.CreateTerminal(node.StateId.TokenType);
                
                IStateBuilder<char, TokenizerStateId<TTokenType>> fallbackTerminalStateBuilder = context.GetStateBuilder(terminalStateId);

                currentNodeStateBuilder.Default(fallbackTerminalStateBuilder);

                fallbackStateId = terminalStateId;
            }
            else
            {
                // Add a default in case we don't match on the nodes key
                IStateBuilder<char, TokenizerStateId<TTokenType>> fallbackStateBuilder = context.GetStateBuilder(fallbackStateId);

                currentNodeStateBuilder.Default(fallbackStateBuilder);
            }

            foreach (TokenTreeNode<TTokenType> childNode in node)
            {
                BuildNodeTransitions(childNode, currentNodeStateBuilder, context, fallbackStateId);
            }
        }

        private static void BuildTransitionsForNodeWithNoChildren(
            TokenTreeNode<TTokenType> node,
            IStateBuilder<char, TokenizerStateId<TTokenType>> previousNodeStateBuilder, 
            TokenizerStateMachineContext context)
        {
            if (node.HasChildren)
                throw new InvalidOperationException("node has children");
            if (!node.IsEndOfToken)
                throw new InvalidOperationException("node is not an end of token");
            
            // If this node had no children, then we need to switch to a dummy state
            // to ensure the character is read
            IStateBuilder<char, TokenizerStateId<TTokenType>> currentNodeStateBuilder =
                context.GetStateBuilder(node.StateId);
            
            previousNodeStateBuilder.When(node.Key, currentNodeStateBuilder);

            // Default to terminal state for the final node
            TokenizerStateId<TTokenType> terminalStateId = TokenizerStateId<TTokenType>.CreateTerminal(node.StateId.TokenType);
            IStateBuilder<char, TokenizerStateId<TTokenType>> terminalStateBuilder = context.GetStateBuilder(terminalStateId);
            currentNodeStateBuilder.Default(terminalStateBuilder);

            // If this whole branch is whitespace, then add checks for WhiteSpace situations
            if (node.IsWhiteSpaceToRoot)
            {
                TokenTreeNode<TTokenType> rootNode = node.RootNode;

                // We might be starting another one of these branches \r\r\n = \r {whitespace} \r\n {record}
                previousNodeStateBuilder.When(rootNode.Key, context.EndOfWhiteSpaceStateBuilder);

                // Or there could be some other type of whitespace character \r\t = {whitespace}
                previousNodeStateBuilder.When(c => char.IsWhiteSpace(c), context.WhiteSpaceStateBuilder);
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

        private class TokenizerStateMachineContext(TokenTree<TTokenType> tree)
        {
            public readonly TokenTree<TTokenType> Tree = tree;

            private readonly Dictionary<TokenizerStateId<TTokenType>, IStateBuilder<char, TokenizerStateId<TTokenType>>>
                _stateBuilders = new();

            public IStateBuilder<char, TokenizerStateId<TTokenType>> WhiteSpaceStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.WhiteSpace);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> NumberStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.Number);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> TextStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.Text);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> EndOfWhiteSpaceStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.EndOfWhiteSpace);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> EndOfNumberStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.EndOfNumber);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> EndOfTextStateBuilder => field ??=
                GetStateBuilder(TokenizerStateId<TTokenType>.EndOfText);

            public IStateBuilder<char, TokenizerStateId<TTokenType>> GetStateBuilder(
                TokenizerStateId<TTokenType> stateId) => _stateBuilders.GetOrAdd(stateId, CreateStateBuilder);

            private static IStateBuilder<char, TokenizerStateId<TTokenType>> CreateStateBuilder(
                TokenizerStateId<TTokenType> stateId) =>
                new StateBuilder<char, TokenizerStateId<TTokenType>>(stateId, stateId.IsTerminal);
        }
}
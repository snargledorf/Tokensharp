using System.Diagnostics;
using System.Linq.Expressions;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine
{
    internal static class TokenizerStateMachine<TTokenType>
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        internal static readonly TokenizerState<TTokenType> StartState;

        static TokenizerStateMachine()
        {
            DuplicateTokenLexemeException<TTokenType>.ThrowIfDuplicateLexemes();

            TokenTree<TTokenType> tree = TTokenType.TokenTypes.ToTokenTree();

            var startStateBuilder = new TokenizerStateBuilder<TTokenType>(TokenType<TTokenType>.Start);
            
            BuildTreeTransitions(startStateBuilder, tree);
            
            BuildWhiteSpaceState(startStateBuilder, tree);

            BuildNumberState(startStateBuilder);

            BuildTextState(startStateBuilder, tree);
            
            StartState = startStateBuilder.Build();
        }

        public static void BuildTreeTransitions(TokenizerStateBuilder<TTokenType> builder, TokenTree<TTokenType> tree)
        {
            // Start at the generated token types
            TTokenType generatedTokenType = TokenType<TTokenType>.StartOfGeneratedTokenTypes.Next();
            foreach (TokenTreeNode<TTokenType> node in tree)
                BuildNodeTransitions(node, builder, ref generatedTokenType);
        }

        private static void BuildWhiteSpaceState(TokenizerStateBuilder<TTokenType> startBuilder,
            TokenTree<TTokenType> tree)
        {
            TokenizerStateBuilder<TTokenType> whiteSpaceBuilder = startBuilder.When(c => char.IsWhiteSpace(c), TokenType<TTokenType>.WhiteSpace);
            BuildTextOrWhiteSpaceState(whiteSpaceBuilder, tree, true);
        }

        private static void BuildTextState(TokenizerStateBuilder<TTokenType> startBuilder,
            TokenTree<TTokenType> tree)
        {
            TokenizerStateBuilder<TTokenType> textBuilder = startBuilder.Default(TokenType<TTokenType>.Text);
            BuildTextOrWhiteSpaceState(textBuilder, tree, false);
        }

        private static void BuildTextOrWhiteSpaceState(TokenizerStateBuilder<TTokenType> builder,
            TokenTree<TTokenType> tree, bool whiteSpace)
        {
            TTokenType nextTokenType =
                whiteSpace ? TokenType<TTokenType>.EndOfWhiteSpace : TokenType<TTokenType>.EndOfText;

            foreach (TokenTreeNode<TTokenType> node in tree)
            {
                if (whiteSpace)
                {
                    // If we have a token type that starts with a whitespace character
                    // then we need to add a potential switch to the end of white space
                    if (node.IsWhiteSpaceToRoot())
                        builder.When(node.Key, nextTokenType);
                }
                else if (!node.IsWhiteSpaceToRoot())
                    builder.When(node.Key, nextTokenType);
            }

            // Add a check if the next character is or is not another white space character
            // depending if we are currently on white space or not
            Expression<Func<char, bool>> isWhiteSpaceExpression = whiteSpace
                ? GetConditionExpression(c => !char.IsWhiteSpace(c))
                : GetConditionExpression(c => char.IsWhiteSpace(c) || char.IsDigit(c));

            builder.When(isWhiteSpaceExpression, nextTokenType);
        }

        private static void BuildNumberState(TokenizerStateBuilder<TTokenType> startBuilder)
        {
            TokenizerStateBuilder<TTokenType> numberBuilder = startBuilder.When(c => char.IsDigit(c), TokenType<TTokenType>.Number);
            numberBuilder.When(c => !char.IsDigit(c), TokenType<TTokenType>.EndOfNumber);
        }

        private static void BuildNodeTransitions(
            TokenTreeNode<TTokenType> node,
            TokenizerStateBuilder<TTokenType> currentTokenizerStateBuilder,
            ref TTokenType generatedTokenType)
        {
            if (node.TokenType is not null)
            {
                BuildTransitionForNodeWithToken(node, currentTokenizerStateBuilder, ref generatedTokenType);
            }
            else
            {
                BuildTransitionsForNodeWithNoTokenType(node, currentTokenizerStateBuilder, ref generatedTokenType);
            }
        }

        private static void BuildTransitionForNodeWithToken(TokenTreeNode<TTokenType> node, TokenizerStateBuilder<TTokenType> currentTokenizerStateBuilder, ref TTokenType generatedTokenType)
        {
            if (node.HasChildren)
            {
                BuildTransitionsForNodeWithTokenAndChildren(node, currentTokenizerStateBuilder, ref generatedTokenType);
            }
            else
            {
                BuildTransitionsForNodeWithTokenAndNoChildren(node, currentTokenizerStateBuilder, ref generatedTokenType);
            }
        }

        private static void BuildTransitionsForNodeWithTokenAndChildren(
            TokenTreeNode<TTokenType> node,
            TokenizerStateBuilder<TTokenType> currentTokenizerStateBuilder, 
            ref TTokenType generatedTokenType)
        {
            if (node.TokenType is null)
                throw new InvalidOperationException("node has no token type");
            if (!node.HasChildren)
                throw new InvalidOperationException("node has no children");
            
            // Build follow-up states to check for longer control strings
            //-------------------------------------------------------------------------------
            // Ex 2. <Foo vs. <FooB vs. <FooBar

            TokenizerStateBuilder<TTokenType> subTokenizerStateBuilder = 
                currentTokenizerStateBuilder.When(node.Key, generatedTokenType);

            // Move to the next token type
            generatedTokenType = generatedTokenType.Next();

            foreach (TokenTreeNode<TTokenType> childNode in node)
                BuildNodeTransitions(childNode, subTokenizerStateBuilder, ref generatedTokenType);
            
            subTokenizerStateBuilder.Default(node.TokenType);
        }

        private static void BuildTransitionsForNodeWithTokenAndNoChildren(TokenTreeNode<TTokenType> node, TokenizerStateBuilder<TTokenType> currentTokenizerStateBuilder, ref TTokenType generatedTokenType)
        {
            if (node.TokenType is null)
                throw new InvalidOperationException("node has no token type");
            if (node.HasChildren)
                throw new InvalidOperationException("node has children");
            
            // If this node had no children, then we need to switch to a dummy state
            // to ensure the character is read
            TokenizerStateBuilder<TTokenType> subStateBuilder = 
                currentTokenizerStateBuilder.When(node.Key, generatedTokenType);

            // Add a default in case we don't match on the nodes key
            TTokenType defaultTokenType = node.IsWhiteSpaceToRoot()
                ? TokenType<TTokenType>.WhiteSpace
                : TokenType<TTokenType>.Text;

            TokenizerStateBuilder<TTokenType> defaultStateBuilder = currentTokenizerStateBuilder.Default(defaultTokenType);
            BuildTextOrWhiteSpaceState(defaultStateBuilder, node.Tree, node.IsWhiteSpaceToRoot());

            // The dummy state just defaults to the final state from the node Value
            subStateBuilder.Default(node.TokenType);

            // Move to the next token type
            generatedTokenType = generatedTokenType.Next();

            // If this whole branch is whitespace then add checks for WhiteSpace situations
            if (node.IsWhiteSpaceToRoot())
            {
                TokenTreeNode<TTokenType> rootNode = node.RootNode;

                // We might be starting another one of these branches \r\r\n = \r {whitespace} \r\n {record}
                currentTokenizerStateBuilder.When(rootNode.Key, TokenType<TTokenType>.EndOfWhiteSpace);

                // Or there could be some other type of whitespace character \r\t = {whitespace}
                currentTokenizerStateBuilder.When(c => char.IsWhiteSpace(c), TokenType<TTokenType>.WhiteSpace);
            }
        }

        private static void BuildTransitionsForNodeWithNoTokenType(TokenTreeNode<TTokenType> node,
            TokenizerStateBuilder<TTokenType> currentTokenizerStateBuilder, ref TTokenType generatedTokenType)
        {
            if (node.TokenType is not null)
                throw new InvalidOperationException("node has a token type");
            if (!node.HasChildren)
                throw new InvalidOperationException("node has no children");
            
            // If we don't have a value then this is just an intermediate node and must have children
            currentTokenizerStateBuilder = currentTokenizerStateBuilder.When(node.Key, generatedTokenType);

            foreach (TokenTreeNode<TTokenType> childNode in node)
            {
                generatedTokenType = generatedTokenType.Next();
                BuildNodeTransitions(childNode, currentTokenizerStateBuilder, ref generatedTokenType);
            }
        }

        private static Expression<Func<char, bool>> GetConditionExpression(Expression<Func<char, bool>> expression)
        {
            return expression;
        }
    }
}

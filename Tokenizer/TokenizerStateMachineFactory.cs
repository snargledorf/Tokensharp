using System.Linq.Expressions;
using FastState;
using Tokenizer.TokenTree;

namespace Tokenizer
{
    internal static class TokenizerStateMachineFactory
    {
        internal static StateMachine<TTokenType, char> Create<TTokenType>() 
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            DuplicateTokenLexemeException<TTokenType>.ThrowIfDuplicateLexemes();
            
            TokenTree<TTokenType> tree = CreateTokenTree<TTokenType>();
            
            return new StateMachine<TTokenType, char>(builder =>
            {
                BuildStartState(builder, tree);
                BuildWhiteSpaceState(builder, tree);
                BuildTextState(builder, tree);
                BuildNumberState(builder, tree);
            });
        }

        private static void BuildStartState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder,
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            IStateTransitionMapBuilder<TTokenType, char> startBuilder = builder.From(TokenType<TTokenType>.Start);

            // Start at the generated token types
            TTokenType tokenType = TokenType<TTokenType>.StartOfGeneratedTokenTypes.Next();
            foreach (TokenTreeNode<TTokenType> node in tree)
                BuildTransitions(node, startBuilder, ref tokenType);

            startBuilder
                .When(c => char.IsWhiteSpace(c), TokenType<TTokenType>.WhiteSpace)
                .When(c => char.IsDigit(c), TokenType<TTokenType>.Number)
                .Default(TokenType<TTokenType>.Text);
        }

        private static void BuildWhiteSpaceState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder, 
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            BuildTextOrWhiteSpaceState(builder, tree, true);
        }

        private static void BuildTextState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder,
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            BuildTextOrWhiteSpaceState(builder, tree, false);
        }

        private static void BuildTextOrWhiteSpaceState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder, 
            TokenTree<TTokenType> tree, 
            bool whiteSpace)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            TTokenType currentTokenType = whiteSpace ? TokenType<TTokenType>.WhiteSpace : TokenType<TTokenType>.Text;
            TTokenType nextTokenType = whiteSpace ? TokenType<TTokenType>.EndOfWhiteSpace : TokenType<TTokenType>.EndOfText;

            IStateTransitionMapBuilder<TTokenType, char> textOrWhiteSpaceBuilder = builder.From(currentTokenType);

            foreach (TokenTreeNode<TTokenType> node in tree)
            {
                if (whiteSpace)
                {
                    // If we have a token type that starts with a whitespace character
                    // then we need to add a potential switch to the end of white space
                    if (node.IsWhiteSpaceToRoot())
                        textOrWhiteSpaceBuilder.When(node.Key, nextTokenType);
                }
                else if (!node.IsWhiteSpaceToRoot())
                    textOrWhiteSpaceBuilder.When(node.Key, nextTokenType);
            }

            // Add a check if the next character is or is not another white space character
            // depending if we are currently on white space or not
            Expression<Func<char, bool>> isWhiteSpaceExpression = whiteSpace
                ? GetExpression(c => !char.IsWhiteSpace(c))
                : GetExpression(c => char.IsWhiteSpace(c));

            textOrWhiteSpaceBuilder.When(isWhiteSpaceExpression, nextTokenType);
        }

        private static Expression<Func<char, bool>> GetExpression(Expression<Func<char, bool>> expression)
        {
            return expression;
        }

        private static void BuildNumberState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder,
            TokenTree<TTokenType> tree) 
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            IStateTransitionMapBuilder<TTokenType,char> numberBuilder = builder.From(TokenType<TTokenType>.Number);
            
            numberBuilder.When(c => !char.IsDigit(c), TokenType<TTokenType>.EndOfNumber);
        }

        private static void BuildTransitions<TTokenType>(TokenTreeNode<TTokenType> node,
            IStateTransitionMapBuilder<TTokenType, char> currentMapBuilder, ref TTokenType tokenType)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            // If this node has a state then it should be treated as a final node
            // This breaks instances where a control string may be the start of another control string
            // Ex. " vs. "", " will be caught but not ""
            // Ex 2. <Foo vs. <FooBar, <FooBar will never be hit since <Foo finished first
            // Need to build states to check for longer control strings
            if (node.TokenType is not null)
            {
                if (node.HasChildren)
                {
                    // Build follow-up states to check for longer control strings
                    //-------------------------------------------------------------------------------
                    // Ex 2. <Foo vs. <FooB vs. <FooBar
                    //
                    // We already matched on <Foo
                    // so now we need states to possibly fall back to <Foo if <FooB fails
                    // or <FooB if <FooBar fails
                    //
                    // currentMapBuilder.When('o', [State id for second o]);
                    //
                    // builder.From([State id for second o])
                    //  .When('B', [State id for B])
                    //  .Default(node.Value); // Matching on anything else sets our state to <Foo
                    //
                    // builder.From([State id for second B])
                    //  .When('a', [State id for a])
                    //  .Default([<FooB node].Value);
                    //
                    // builder.From([State id for a])
                    //  .When('r', [<FooBar node].Value)
                    //  .Default([<FooB node].Value);
                    //
                    // etc.

                    currentMapBuilder.When(node.Key, tokenType);
                    
                    IStateTransitionMapBuilder<TTokenType, char>? subStateBuilder =
                        currentMapBuilder.StateMachineTransitionMapBuilder.From(tokenType);

                    // Move to the next token type
                    tokenType = tokenType.Next();

                    foreach (TokenTreeNode<TTokenType> childNode in node)
                    {
                        BuildTransitions(childNode, subStateBuilder, ref tokenType);
                        subStateBuilder.Default(node.TokenType);
                    }
                }
                else
                {
                    // If this node had no children, then we need to switch to a dummy state
                    // to ensure the character is read
                    currentMapBuilder.When(node.Key, tokenType);

                    // Add a default in case we don't match on the nodes key
                    TTokenType defaultTokenType = node.IsWhiteSpaceToRoot()
                        ? TokenType<TTokenType>.WhiteSpace
                        : TokenType<TTokenType>.Text;

                    currentMapBuilder.Default(defaultTokenType);

                    // The dummy state just defaults to the final state from the node Value
                    currentMapBuilder.StateMachineTransitionMapBuilder.From(tokenType)
                        .Default(node.TokenType);

                    // Move to the next token type
                    tokenType = tokenType.Next();

                    // If this whole branch is whitespace then add checks for WhiteSpace situations
                    if (node.IsWhiteSpaceToRoot())
                    {
                        TokenTreeNode<TTokenType> rootNode = node.RootNode;

                        // We might be starting another one of these branches \r\r\n = \r {whitespace} \r\n {record}
                        currentMapBuilder.When(rootNode.Key, TokenType<TTokenType>.EndOfWhiteSpace);

                        // Or there could be some other type of whitespace character \r\t = {whitespace}
                        currentMapBuilder.When(c => char.IsWhiteSpace(c), TokenType<TTokenType>.WhiteSpace);
                    }
                }
            }
            else
            {
                // If we don't have a value then this is just an intermediate node and must have children
                currentMapBuilder.When(node.Key, tokenType);

                currentMapBuilder = currentMapBuilder.StateMachineTransitionMapBuilder.From(tokenType);

                foreach (TokenTreeNode<TTokenType> childNode in node)
                {
                    tokenType = tokenType.Next();
                    BuildTransitions(childNode, currentMapBuilder, ref tokenType);
                }
            }
        }

        private static TokenTree<TTokenType> CreateTokenTree<TTokenType>() 
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            var tree = new TokenTree<TTokenType>();

            foreach (TTokenType tokenDefinition in TTokenType.TokenTypes)
            {
                TokenTreeNode<TTokenType>? currentNode = null;
                foreach (char nodeKey in tokenDefinition.Lexeme)
                {
                    if (currentNode is null)
                    {
                        if (!tree.TryGetChild(nodeKey, out currentNode))
                            tree.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey));
                    }
                    else
                    {
                        if (currentNode.TryGetChild(nodeKey, out TokenTreeNode<TTokenType>? nextNode))
                        {
                            currentNode = nextNode;
                        }
                        else
                        {
                            currentNode.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey, currentNode));
                        }
                    }
                }

                // This is the final node in this branch, set the state
                if (currentNode is { } endNode)
                    endNode.TokenType = tokenDefinition;
            }
            
            return tree;
        }
    }
}

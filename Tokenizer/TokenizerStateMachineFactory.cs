using System.Linq.Expressions;
using FastState;
using Tokenizer.TokenTree;

namespace Tokenizer
{
    internal static class TokenizerStateMachineFactory
    {
        internal static StateMachine<TTokenType, char> Create<TTokenType>(
            IEnumerable<TTokenType> tokenDefinitions) 
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            TokenTree<TTokenType> tree = CreateTokenTree(tokenDefinitions);

            /* This is the Goal
             * 
             * The start builder is done I think
            builder.From(FlexableTokenizerTokenState.Start)
                .When(',', FlexableTokenizerTokenState.EndOfFieldDelimiter)
                .When('\r', FlexableTokenizerTokenState.StartOfEndOfRecord)
                .When('"', FlexableTokenizerTokenState.EndOfFieldDelimiter)
                .When((c) => char.IsWhiteSpace(c), FlexableTokenizerTokenState.WhiteSpace)
                .Default(FlexableTokenizerTokenState.Text);

             * This needs more testing, should be built when building start state
            builder.From(FlexableTokenizerTokenState.StartOfEndOfRecord)
                .When('\n', FlexableTokenizerTokenState.EndOfEndOfRecord)
                .When('\r', FlexableTokenizerTokenState.EndOfWhiteSpace)
                .When((c) => char.IsWhiteSpace(c), FlexableTokenizerTokenState.WhiteSpace);

             * Currently not possible. Need to work around currently, but may be able to improve.
            builder.From(FlexableTokenizerTokenState.WhiteSpace)
                .When((c) => c == '\r' || !char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfWhiteSpace);

            builder.From(FlexableTokenizerTokenState.Text)
                .When((c) => c == ',' || c == '"' || char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfText);
            */
            return new StateMachine<TTokenType, char>(builder =>
            {
                BuildStartState(builder, tree);
                BuildWhiteSpaceState(builder, tree);
                BuildTextState(builder, tree);
            });
        }


        // This method builds the start state and any child states
        //builder.From(FlexableTokenizerTokenState.Start)
        //    .When(',', FlexableTokenizerTokenState.EndOfFieldDelimiter)
        //    .When('\r', FlexableTokenizerTokenState.StartOfEndOfRecord)
        //    .When('"', FlexableTokenizerTokenState.EndOfFieldDelimiter)
        //    .When((c) => char.IsWhiteSpace(c), FlexableTokenizerTokenState.WhiteSpace)
        //    .Default(FlexableTokenizerTokenState.Text);

        // * This needs more testing, should be built when building start state
        //builder.From(FlexableTokenizerTokenState.StartOfEndOfRecord)
        //    .When('\n', FlexableTokenizerTokenState.EndOfEndOfRecord)
        //    .When('\r', FlexableTokenizerTokenState.EndOfWhiteSpace)
        //    .When((c) => char.IsWhiteSpace(c), FlexableTokenizerTokenState.WhiteSpace);
        private static void BuildStartState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder,
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            IStateTransitionMapBuilder<TTokenType, char> startBuilder = builder.From(TokenType<TTokenType>.Start);

            // Start after the maximum token type
            TTokenType tokenType = TTokenType.Maximum.Next();
            foreach (TokenTreeNode<TTokenType> node in tree)
                BuildTransitions(node, startBuilder, ref tokenType);

            startBuilder
                .When(c => char.IsWhiteSpace(c), TokenType<TTokenType>.WhiteSpace)
                .Default(TokenType<TTokenType>.Text);
        }


        // Builds this -
        // .When('\r', FlexableTokenizerTokenState.EndOfWhiteSpace)
        // .When(!char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfWhiteSpace)
        private static void BuildWhiteSpaceState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder, 
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            BuildTextOrWhiteSpaceState(builder, tree, true);
        }

        // Builds this -
        // .When(',', FlexableTokenizerTokenState.EndOfText)
        // .When('"', FlexableTokenizerTokenState.EndOfText)
        // .When(char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfText)
        private static void BuildTextState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder,
            TokenTree<TTokenType> tree)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            BuildTextOrWhiteSpaceState(builder, tree, false);
        }


        // Builds this -
        // .When(',', FlexableTokenizerTokenState.EndOfText)
        // .When('"', FlexableTokenizerTokenState.EndOfText)
        // .When(char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfText)
        // Or this -
        // .When('\r', FlexableTokenizerTokenState.EndOfText)
        // .When(!char.IsWhiteSpace(c), FlexableTokenizerTokenState.EndOfWhiteSpace)
        private static void BuildTextOrWhiteSpaceState<TTokenType>(
            IStateMachineTransitionMapBuilder<TTokenType, char> builder, 
            TokenTree<TTokenType> tree, 
            bool whiteSpace)
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            TTokenType currentTokenType = whiteSpace ? TokenType<TTokenType>.WhiteSpace : TokenType<TTokenType>.Text;
            TTokenType nextTokenType = whiteSpace ? TokenType<TTokenType>.EndOfWhiteSpace : TokenType<TTokenType>.EndOfText;

            IStateTransitionMapBuilder<TTokenType, char> textBuilder = builder.From(currentTokenType);

            foreach (TokenTreeNode<TTokenType> node in tree)
            {
                if (whiteSpace)
                {
                    if (node.IsWhiteSpaceToRoot())
                        textBuilder.When(node.Key, nextTokenType);
                }
                else if (!node.IsWhiteSpaceToRoot())
                    textBuilder.When(node.Key, nextTokenType);
            }

            Expression<Func<char, bool>> isWhiteSpaceExpression = whiteSpace
                ? GetExpression(c => !char.IsWhiteSpace(c))
                : GetExpression(c => char.IsWhiteSpace(c));

            textBuilder.When(isWhiteSpaceExpression, nextTokenType);
        }

        private static Expression<Func<char, bool>> GetExpression(Expression<Func<char, bool>> expression)
        {
            return expression;
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
            if (node.State is not null)
            {
                if (node.HasChildren)
                {
                    // Build follow-up states to check for longer control strings
                    // Ex. " vs. ""
                    //
                    // We already matched on ", so now we need a state to possibly fall back to " if "" doesn't work out
                    //
                    // builder.From([State id for "])
                    //  .When('"', [Escape child node].Value) // IE. FlexableTokenizerTokenState.EndOfEscape
                    //  .Default(node.Value);
                    //
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
                    //  .Default(node.Value);
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
                        subStateBuilder.Default(node.State);
                    }
                }
                else
                {
                    // If this node had no children, then we need to switch to a dummy state
                    // to ensure the character is read
                    currentMapBuilder.When(node.Key, tokenType);

                    // The dummy state just defaults to the final state from the node Value
                    currentMapBuilder.StateMachineTransitionMapBuilder.From(tokenType)
                        .Default(node.State);

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

        private static TokenTree<TTokenType> CreateTokenTree<TTokenType>(
            IEnumerable<TTokenType> tokenDefinitions) 
            where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
        {
            var tree = new TokenTree<TTokenType>();

            foreach (TTokenType tokenDefinition in tokenDefinitions)
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
                    endNode.State = tokenDefinition;
            }
            
            return tree;
        }
    }
}

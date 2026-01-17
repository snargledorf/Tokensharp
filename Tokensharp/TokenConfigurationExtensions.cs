using Tokensharp.TokenTree;

namespace Tokensharp;

internal static class TokenConfigurationExtensions
{
    extension<TTokenType>(IEnumerable<LexemeToTokenType<TTokenType>> lexemesToTokens)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        internal ITokenTreeNode<TTokenType> ToTokenTree()
        {
            var rootNode = new TokenTreeNodeBuilder<TTokenType>('\0');

            foreach (LexemeToTokenType<TTokenType> tokenDefinition in lexemesToTokens.OrderBy(t => t.Lexeme.Length))
            {
                ITokenTreeNodeBuilder<TTokenType> currentNodeBuilder = rootNode;
            
                foreach (char nodeKey in tokenDefinition.Lexeme)
                {
                    if (currentNodeBuilder.TryGetChild(nodeKey, out ITokenTreeNodeBuilder<TTokenType>? nextNode))
                    {
                        currentNodeBuilder = nextNode;
                    }
                    else
                    {
                        currentNodeBuilder = currentNodeBuilder.AddChild(nodeKey);
                    }
                }

                // This is the final node in this branch, set the token type to terminal
                currentNodeBuilder.TokenType = tokenDefinition.TokenType;
            }
            
            return rootNode.Build();
        }
    }
}
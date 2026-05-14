using Tokensharp.Trie;

namespace Tokensharp;

internal static class TokenConfigurationExtensions
{
    extension<T>(IEnumerable<LexemeToTokenType<T>> lexemesToTokens) where T : class
    {
        internal TrieNode<T> ToFastTrie()
        {
            var rootNode = new TrieNodeBuilder<T>('\0');

            foreach (LexemeToTokenType<T> tokenDefinition in lexemesToTokens.OrderBy(t => t.Lexeme.Length))
            {
                TrieNodeBuilder<T> currentNodeBuilder = rootNode;
            
                foreach (char nodeKey in tokenDefinition.Lexeme)
                {
                    if (currentNodeBuilder.TryGetChild(nodeKey, out TrieNodeBuilder<T>? nextNode))
                    {
                        currentNodeBuilder = nextNode;
                    }
                    else
                    {
                        currentNodeBuilder = currentNodeBuilder.Add(nodeKey);
                    }
                }

                // This is the final node in this branch, set the token type to terminal
                currentNodeBuilder.Value = tokenDefinition.TokenType;
            }
            
            return rootNode.Build();
        }
    }
}
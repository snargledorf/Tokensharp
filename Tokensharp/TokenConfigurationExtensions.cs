using System.Text;
using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

internal static class TokenConfigurationExtensions
{
    extension<TTokenType>(ITokenConfiguration<TTokenType> tokenConfiguration)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        internal TokenTree<TTokenType> ToTokenTree()
        {
            var tree = new TokenTree<TTokenType>();

            var stateNameBuilder = new StringBuilder();

            foreach (LexemeToTokenType<TTokenType> tokenDefinition in tokenConfiguration)
            {
                TokenTreeNode<TTokenType>? currentNode = null;
            
                stateNameBuilder.Append($"({tokenDefinition.Lexeme})_");
            
                foreach (char nodeKey in tokenDefinition.Lexeme)
                {
                    stateNameBuilder.Append(nodeKey);
                
                    if (currentNode is null)
                    {
                        if (tree.TryGetChild(nodeKey, out currentNode))
                            continue;
                    
                        TokenizerStateId<TTokenType> stateId = TokenizerStateId<TTokenType>.Create(stateNameBuilder.ToString(), tokenDefinition.TokenType);
                        tree.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey, stateId, tree));
                    }
                    else
                    {
                        if (currentNode.TryGetChild(nodeKey, out TokenTreeNode<TTokenType>? nextNode))
                        {
                            currentNode = nextNode;
                        }
                        else
                        {
                            TokenizerStateId<TTokenType> stateId = TokenizerStateId<TTokenType>.Create(stateNameBuilder.ToString(), tokenDefinition.TokenType);
                            currentNode.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey, stateId, tree, currentNode));
                        }
                    }
                }

                // This is the final node in this branch, set the token type to terminal
                if (currentNode is { } endNode)
                    endNode.IsEndOfToken = true;
            
                stateNameBuilder.Clear();
            }
            
            return tree;
        }
    }
}
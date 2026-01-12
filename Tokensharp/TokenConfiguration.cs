using System.Text;
using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

public partial class TokenConfiguration<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<string, TTokenType> _tokenDefinitions = new();
    
    internal TokenTree<TTokenType> ToTokenTree()
    {
        var tree = new TokenTree<TTokenType>();

        var stateNameBuilder = new StringBuilder();

        foreach (KeyValuePair<string, TTokenType> tokenDefinition in _tokenDefinitions)
        {
            TokenTreeNode<TTokenType>? currentNode = null;
            
            stateNameBuilder.Append($"({tokenDefinition.Key})_");
            
            foreach (char nodeKey in tokenDefinition.Key)
            {
                stateNameBuilder.Append(nodeKey);
                
                if (currentNode is null)
                {
                    if (tree.TryGetChild(nodeKey, out currentNode))
                        continue;
                    
                    TokenizerStateId<TTokenType> stateId = TokenizerStateId<TTokenType>.Create(stateNameBuilder.ToString(), tokenDefinition.Value);
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
                        TokenizerStateId<TTokenType> stateId = TokenizerStateId<TTokenType>.Create(stateNameBuilder.ToString(), tokenDefinition.Value);
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
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal static class TokenTypesExtensions
{
    public static TokenTree<TTokenType> ToTokenTree<TTokenType>(this IEnumerable<TTokenType> tokenDefinitions) 
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
                        tree.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey, tree));
                }
                else
                {
                    if (currentNode.TryGetChild(nodeKey, out TokenTreeNode<TTokenType>? nextNode))
                    {
                        currentNode = nextNode;
                    }
                    else
                    {
                        currentNode.AddChild(currentNode = new TokenTreeNode<TTokenType>(nodeKey, tree, currentNode));
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
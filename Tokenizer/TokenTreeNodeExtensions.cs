using Tokenizer.TokenTree;

namespace Tokenizer;

public static class TokenTreeNodeExtensions
{
    public static bool IsWhiteSpaceToRoot<TTokenType>(this TokenTreeNode<TTokenType> node)
    {
        return char.IsWhiteSpace(node.Key) && (node.Parent?.IsWhiteSpaceToRoot() ?? true);
    }
}
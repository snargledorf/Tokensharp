using Tokensharp.TokenTree;

namespace Tokensharp;

internal static class TokenTreeNodeExtensions
{
    extension<TTokenType>(TokenTreeNode<TTokenType> node) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        public bool IsWhiteSpaceToRoot => char.IsWhiteSpace(node.Key) && (node.Parent?.IsWhiteSpaceToRoot ?? true);

        public bool IsDigitsToRoot => char.IsDigit(node.Key) && (node.Parent?.IsDigitsToRoot ?? true);
    }
}
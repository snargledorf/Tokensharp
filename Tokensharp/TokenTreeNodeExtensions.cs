using Tokensharp.TokenTree;

namespace Tokensharp;

internal static class TokenTreeNodeExtensions
{
    extension<TTokenType>(ITokenTreeNode<TTokenType> node) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        public bool IsWhiteSpaceToRoot => char.IsWhiteSpace(node.Character) && (node.Parent?.IsWhiteSpaceToRoot ?? true);

        public bool IsDigitsToRoot => char.IsDigit(node.Character) && (node.Parent?.IsDigitsToRoot ?? true);

        public bool HasPrefixTokenType(TokenType<TTokenType> tokenType) => (node.Parent?.TokenType == tokenType) || (node.Parent?.HasPrefixTokenType(tokenType) ?? false);

        public HashSet<char> AllCharacters => [..GetAllCharactersForNodeRecursive(node).Distinct()];
    }

    private static IEnumerable<char>
        GetAllCharactersForNodeRecursive<TTokenType>(ITokenTreeNode<TTokenType> tokenTreeNode)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        if (tokenTreeNode.Character != '\0')
            yield return tokenTreeNode.Character;

        foreach (char c in tokenTreeNode.SelectMany(GetAllCharactersForNodeRecursive))
            yield return c;
    }
}
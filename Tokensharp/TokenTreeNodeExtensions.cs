using Tokensharp.TokenTree;

namespace Tokensharp;

internal static class TokenTreeNodeExtensions
{
    extension<TTokenType>(ITokenTreeNode<TTokenType> node) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        public bool IsWhiteSpaceToRoot =>
            char.IsWhiteSpace(node.Character) && (node.Parent?.IsWhiteSpaceToRoot ?? true);

        public bool IsDigitsToRoot => char.IsDigit(node.Character) && (node.Parent?.IsDigitsToRoot ?? true);

        public bool HasPrefixTokenType(TokenType<TTokenType> tokenType) => node.Parent?.TokenType == tokenType ||
                                                                           (node.Parent
                                                                                ?.HasPrefixTokenType(tokenType) ??
                                                                            false);

        public HashSet<char> AllCharacters => TokenTreeAllCharactersHelper<TTokenType>.GetAllCharacters(node);
    }
}

internal static class TokenTreeAllCharactersHelper<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static HashSet<char>? _allCharacters;
    
    public static HashSet<char> GetAllCharacters(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        return _allCharacters ??= [..GetAllCharactersForNodeRecursive(tokenTreeNode).Distinct()];
    }

    private static IEnumerable<char> GetAllCharactersForNodeRecursive(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        if (tokenTreeNode.Character != '\0')
            yield return tokenTreeNode.Character;

        foreach (char c in tokenTreeNode.SelectMany(GetAllCharactersForNodeRecursive))
            yield return c;
    }
}
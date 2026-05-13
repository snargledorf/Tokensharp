using Tokensharp.FastTrie;

namespace Tokensharp;

public sealed class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes, bool numbersAreText = false)
    {
        TrieRootNode = lexemeToTokenTypes.ToFastTrie();
        
        NumbersAreText = numbersAreText;
    }

    public bool NumbersAreText { get; }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }
    
    internal TrieNode<TTokenType> TrieRootNode { get; }
}
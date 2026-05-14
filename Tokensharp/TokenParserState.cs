using Tokensharp.Trie;

namespace Tokensharp;

public readonly struct TokenParserState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal readonly TrieNode<TTokenType>? TrieRootNode;
    internal readonly bool NumbersAreText;

    public TokenParserState(TokenConfiguration<TTokenType> tokenConfiguration) 
        : this(tokenConfiguration.TrieRootNode, tokenConfiguration.NumbersAreText)
    {
    }

    internal TokenParserState(TrieNode<TTokenType> trieRootNode, bool numbersAreText)
    {
        TrieRootNode = trieRootNode;
        NumbersAreText = numbersAreText;
    }
}
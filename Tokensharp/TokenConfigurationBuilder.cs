using System.Collections;

namespace Tokensharp;

public class TokenConfigurationBuilder<TTokenType> : IEnumerable<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<string, TTokenType> _tokenDefinitions = new();

    public TokenConfigurationBuilder() : this(Array.Empty<LexemeToTokenType<TTokenType>>())
    {
    }

    public TokenConfigurationBuilder(IEnumerable<TTokenType> tokenTypes)
    {
        foreach (TTokenType tokenType in tokenTypes)
            Add(tokenType);
    }

    public TokenConfigurationBuilder(IEnumerable<LexemeToTokenType<TTokenType>> lexemesToTokens)
    {
        foreach (LexemeToTokenType<TTokenType> lexemeToToken in lexemesToTokens)
            Add(lexemeToToken);
    }
    
    public TTokenType this[string lexeme]
    {
        get => _tokenDefinitions[lexeme];
        set => _tokenDefinitions[lexeme] = value;
    }

    public void Add(string lexeme, TTokenType tokenType)
    {
        if (!_tokenDefinitions.TryAdd(lexeme, tokenType))
            throw new DuplicateLexemeException(lexeme);
    }

    public void Add(TTokenType tokenType)
    {
        if (!_tokenDefinitions.TryAdd(tokenType.Identifier, tokenType))
            throw new DuplicateLexemeException(tokenType.Identifier);
    }

    public void Add(LexemeToTokenType<TTokenType> lexemeToTokenType)
    {
        Add(lexemeToTokenType.Lexeme, lexemeToTokenType.TokenType);
    }

    public bool NumbersAreText { get; set; } = false;

    public bool LexemeConfigured(string lexeme) => _tokenDefinitions.ContainsKey(lexeme);
    
    public TokenConfiguration<TTokenType> Build()
    {
        return new TokenConfiguration<TTokenType>(_tokenDefinitions.Select(kv => new LexemeToTokenType<TTokenType>(kv.Key, kv.Value)), NumbersAreText);
    }

    public IEnumerator<TTokenType> GetEnumerator()
    {
        return _tokenDefinitions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
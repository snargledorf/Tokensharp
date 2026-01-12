using System.Collections;

namespace Tokensharp;

public partial class TokenConfiguration<TTokenType> : IEnumerable<TTokenType>, IEnumerable<LexemeToTokenType<TTokenType>>
{
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
    
    public bool LexemeConfigured(string lexeme) => _tokenDefinitions.ContainsKey(lexeme);
    
    public IEnumerator<TTokenType> GetEnumerator()
    {
        return _tokenDefinitions.Values.GetEnumerator();
    }

    IEnumerator<LexemeToTokenType<TTokenType>> IEnumerable<LexemeToTokenType<TTokenType>>.GetEnumerator()
    {
        return _tokenDefinitions.Select(kvp => new LexemeToTokenType<TTokenType>(kvp.Key, kvp.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
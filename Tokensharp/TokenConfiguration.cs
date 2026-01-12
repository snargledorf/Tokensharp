using System.Collections;

namespace Tokensharp;

public class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IEnumerable<LexemeToTokenType<TTokenType>> _lexemeToTokenTypes;

    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes)
    {
        _lexemeToTokenTypes = lexemeToTokenTypes;
    }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }

    public IEnumerator<LexemeToTokenType<TTokenType>> GetEnumerator()
    {
        return _lexemeToTokenTypes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
namespace Tokensharp;

public interface ITokenConfiguration<TTokenType> : IEnumerable<LexemeToTokenType<TTokenType>>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
}
namespace Tokensharp.Tests;

public record TestCase<TTokenType>(
    TTokenType TokenType,
    string? Lexeme = null,
    bool MoreDataAvailable = false,
    bool ExpectToParse = true)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static implicit operator TestCase<TTokenType>(TTokenType tokenType) => new(tokenType);
}
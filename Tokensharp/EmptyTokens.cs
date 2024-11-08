namespace Tokensharp;

public record EmptyTokens(string Lexeme) : TokenType<EmptyTokens>(Lexeme), ITokenType<EmptyTokens>
{
    public static EmptyTokens Create(string lexeme) => new(lexeme);

    public static IEnumerable<EmptyTokens> TokenTypes { get; } = [];
}
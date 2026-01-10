namespace Tokensharp;

public record RuntimeConfigToken(string Lexeme)  : TokenType<RuntimeConfigToken>(Lexeme), ITokenType<RuntimeConfigToken>
{
    public static RuntimeConfigToken Create(string lexeme) => new(lexeme);

    public static IEnumerable<RuntimeConfigToken> TokenTypes { get; } = [];
}
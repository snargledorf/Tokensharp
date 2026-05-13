namespace Tokensharp;

public record struct LexemeToTokenType<TTokenType>(string Lexeme, TTokenType TokenType);
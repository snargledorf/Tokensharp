namespace Tokensharp;

public record struct LexemeToTokenType<TTokenType>(string Lexeme, TTokenType TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;
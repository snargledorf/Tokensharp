namespace Tokenizer;

public record struct Token<TTokenType>(TTokenType Type, ReadOnlyMemory<char> Lexeme)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;
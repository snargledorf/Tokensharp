namespace Tokenizer;

public record struct Token<TTokenType>(TTokenType Type, int ColumnIndex, int LineIndex, ReadOnlyMemory<char> Value)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;
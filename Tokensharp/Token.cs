namespace Tokensharp;

public readonly record struct Token<TTokenType>(TokenType<TTokenType> Type, string Lexeme)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public int Length => Lexeme?.Length ?? 0;
}
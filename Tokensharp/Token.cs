namespace Tokensharp;

public readonly record struct Token<TTokenType>(TokenType<TTokenType> Type, string Lexeme)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool IsNumber => Type.IsNumber;
    public bool IsText => Type.IsText;
    public bool IsWhiteSpace => Type.IsWhiteSpace;
    
    public int Length => Lexeme?.Length ?? 0;
}
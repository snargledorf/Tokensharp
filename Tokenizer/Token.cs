namespace Tokenizer;

public readonly record struct Token<TTokenType>(TTokenType Type, ReadOnlyMemory<char> Lexeme)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool IsNumber => Type.IsNumber;
    public bool IsText => Type.IsText;
    public bool IsWhiteSpace => Type.IsWhiteSpace;
    
    public int Length => Lexeme.Length;
}
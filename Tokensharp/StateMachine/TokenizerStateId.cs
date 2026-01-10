namespace Tokensharp.StateMachine;

internal record TokenizerStateId<TTokenType>(string Name, bool IsTerminal, TTokenType TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static readonly TokenizerStateId<TTokenType> Start = new("start", false, null!);

    public static readonly TokenizerStateId<TTokenType> WhiteSpace = new(TokenType<TTokenType>.WhiteSpace.Lexeme, false,
        TokenType<TTokenType>.WhiteSpace);

    public static readonly TokenizerStateId<TTokenType> EndOfWhiteSpace =
        CreateTerminal(TokenType<TTokenType>.WhiteSpace);

    public static readonly TokenizerStateId<TTokenType> Number = new(TokenType<TTokenType>.Number.Lexeme, false,
        TokenType<TTokenType>.Number);

    public static readonly TokenizerStateId<TTokenType> EndOfNumber = CreateTerminal(TokenType<TTokenType>.Number);

    public static readonly TokenizerStateId<TTokenType> Text = new(TokenType<TTokenType>.Text.Lexeme, false,
        TokenType<TTokenType>.Text);

    public static readonly TokenizerStateId<TTokenType> EndOfText = CreateTerminal(TokenType<TTokenType>.Text);

    public static TokenizerStateId<TTokenType> CreateTerminal(TTokenType tokenType) =>
        new($"({tokenType.Lexeme})_terminal", true, tokenType);
    
    public static TokenizerStateId<TTokenType> Create(string name, TTokenType tokenType) =>
        new(name, false, tokenType);
}
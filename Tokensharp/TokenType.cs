namespace Tokensharp;

public abstract record TokenType<T>(string Lexeme)
    where T : TokenType<T>, ITokenType<T>
{
    public static readonly T Text = T.Create("text");
    public static readonly T Number = T.Create("number");
    public static readonly T WhiteSpace = T.Create("whitespace");

    private bool? _isUserDefined;
    public bool IsUserDefined => _isUserDefined ??= T.TokenTypes.Contains(this);

    private bool? _isNumber;
    public bool IsNumber => _isNumber ??= this == Number;
    
    private bool? _isText;
    public bool IsText => _isText ??= this == Text;
    
    private bool? _isWhiteSpace;
    public bool IsWhiteSpace => _isWhiteSpace ??= this == WhiteSpace;
}
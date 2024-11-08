namespace Tokenizer;

public abstract record TokenType<T>(string Lexeme)
    where T : TokenType<T>, ITokenType<T>
{
    private static T? _start;
    public static T Start => _start ??= T.Create("start");

    private static T? _text;
    public static T Text => _text ??= Start.Next("text");

    private static T? _digit;
    public static T Number => _digit ??= Text.Next("number");
    
    private static T? _whiteSpace;
    public static T WhiteSpace => _whiteSpace ??= Number.Next("whitespace");
    
    private static T? _endOfText;
    internal static T EndOfText => _endOfText ??= WhiteSpace.Next("end_of_text");
    
    private static T? _endOfNumber;
    internal static T EndOfNumber => _endOfNumber ??= EndOfText.Next("end_of_number");
    
    private static T? _endOfWhiteSpace;
    internal static T EndOfWhiteSpace => _endOfWhiteSpace ??= EndOfNumber.Next("end_of_whitespace");
    
    private static T? _startOfGeneratedTokenTypes;

    internal static T StartOfGeneratedTokenTypes =>
        _startOfGeneratedTokenTypes ??= EndOfWhiteSpace.Next("start_of_generated_types");

    public bool IsGenerated { get; private set; }

    private bool? _isUserDefined;
    public bool IsUserDefined => _isUserDefined ??= T.TokenTypes.Contains(this);

    private bool? _isNumber;
    public bool IsNumber => _isNumber ??= this == Number;
    
    private bool? _isText;
    public bool IsText => _isText ??= this == Text;
    
    private bool? _isWhiteSpace;
    public bool IsWhiteSpace => _isWhiteSpace ??= this == WhiteSpace;

    internal T Next() => Next(Lexeme, true);
    
    public T Next(string lexeme) => Next(lexeme, false);

    private static T Next(string lexeme, bool generated)
    {
        if (generated)
            lexeme += "_g";
        
        var tokenType = T.Create(lexeme);
        
        tokenType.IsGenerated = generated;
        
        return tokenType;
    }
}
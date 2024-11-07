namespace Tokenizer;

public abstract record TokenType<T>(string Lexeme, int Id)
    where T : TokenType<T>, ITokenType<T>
{
    private static T? _start;
    public static T Start => _start ??= T.LastUserDefinedTokenType.Next("start");
    
    private static T? _text;
    public static T Text => _text ??= Start.Next("text");
    
    private static T? _whiteSpace;
    public static T WhiteSpace => _whiteSpace ??= Text.Next("whitespace");
    
    private static T? _endOfText;
    internal static T EndOfText => _endOfText ??= WhiteSpace.Next("end_of_text");
    
    private static T? _endOfWhiteSpace;
    internal static T EndOfWhiteSpace => _endOfWhiteSpace ??= EndOfText.Next("end_of_whitespace");
    
    private static T? _startOfGeneratedTokenTypes;
    internal static T StartOfGeneratedTokenTypes => _startOfGeneratedTokenTypes ??= EndOfWhiteSpace.Next("start_of_generated_types");

    public bool IsGenerated { get; private set; }
    public bool IsDefined => Id < StartOfGeneratedTokenTypes.Id;
    public bool IsUserDefined => Id < Start.Id;

    internal T Next() => Next(Lexeme, true);
    
    public T Next(string lexeme) => Next(lexeme, false);

    private T Next(string lexeme, bool generated)
    {
        int nextId = Id + 1;

        if (generated)
        {
            bool idIsDefined = nextId < StartOfGeneratedTokenTypes.Id;
            if (idIsDefined)
                throw new InvalidOperationException("Generated token id would conflict with previously defined token type");
            
            lexeme += "_g";
        }
        
        var tokenType = T.Create(lexeme, nextId);
        
        tokenType.IsGenerated = generated;
        
        return tokenType;
    }
}
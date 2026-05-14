namespace Tokensharp;

public abstract record TokenType<T>(string Identifier)
    where T : TokenType<T>, ITokenType<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static int _nextTokenTypeIndex = -1;
    private readonly int _tokenTypeIndex = Interlocked.Increment(ref _nextTokenTypeIndex);
    
    public static readonly T Text = T.Create("text");
    public static readonly T Number = T.Create("number");
    public static readonly T WhiteSpace = T.Create("whitespace");

    public int Index => _tokenTypeIndex;
    
    public static implicit operator int(TokenType<T> tokenType) => tokenType._tokenTypeIndex;
}
namespace Tokensharp;

public abstract record TokenType<T>(string Identifier)
    where T : TokenType<T>, ITokenType<T>
{
    public static readonly T Text = T.Create("text");
    public static readonly T Number = T.Create("number");
    public static readonly T WhiteSpace = T.Create("whitespace");
}
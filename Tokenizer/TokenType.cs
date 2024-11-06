namespace Tokenizer;

public abstract record TokenType<T>(string Lexeme, int Id)
    where T : TokenType<T>, ITokenType<T>
{
    public static readonly T Start = T.Create("internal", 0);
    public static readonly T Text = T.Create("text", 1);
    public static readonly T WhiteSpace = T.Create("whitespace", 2);
    internal static readonly T EndOfText = T.Create("end_of_text", 3);
    internal static readonly T EndOfWhiteSpace = T.Create("end_of_whitespace", 4);
    public static readonly T StartOfUserDefinedTokenTypes = T.Create("start_of_user_defined_types", 5);

    public T Next() => Next(Lexeme + "_g");
    public T Next(string lexeme) => T.Create(lexeme, Id + 1);

    public bool IsDefined => Id <= T.Maximum.Id;
}
namespace Tokenizer.Tests;

internal record TestTokenType(string Lexeme, int Id) : TokenType<TestTokenType>(Lexeme, Id), ITokenType<TestTokenType>
{
    public static readonly TestTokenType Fun = new("fun", StartOfUserDefinedTokenTypes.Id);
    public static readonly TestTokenType OpenParenthesis = Fun.Next("(");
    public static readonly TestTokenType ClosedParenthesis = OpenParenthesis.Next(")");

    public static readonly IEnumerable<TestTokenType> Definitions =
    [
        Fun,
        OpenParenthesis,
        ClosedParenthesis
    ];
    
    public static TestTokenType Create(string token, int id) => new(token, id);

    public static TestTokenType Maximum => Definitions.Last();
}
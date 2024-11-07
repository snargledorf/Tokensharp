namespace Tokenizer.Tests;

public record ScriptingLanguageTokenTypes(string Lexeme, int Id) : TokenType<ScriptingLanguageTokenTypes>(Lexeme, Id), ITokenType<ScriptingLanguageTokenTypes>
{
    public static readonly ScriptingLanguageTokenTypes Fun = new("fun", 0);
    public static readonly ScriptingLanguageTokenTypes OpenParenthesis = Fun.Next("(");
    public static readonly ScriptingLanguageTokenTypes CloseParenthesis = OpenParenthesis.Next(")");
    public static readonly ScriptingLanguageTokenTypes OpenBrace = OpenParenthesis.Next("{");
    public static readonly ScriptingLanguageTokenTypes CloseBrace = OpenBrace.Next("}");

    public static readonly IEnumerable<ScriptingLanguageTokenTypes> Definitions =
    [
        Fun,
        OpenParenthesis,
        CloseParenthesis,
        OpenBrace,
        CloseBrace
    ];
    
    public static ScriptingLanguageTokenTypes Create(string token, int id) => new(token, id);
    public static ScriptingLanguageTokenTypes LastUserDefinedTokenType { get; } = Definitions.Last();
}

public class ScriptingLanguageTests : TokenizerTestBase<ScriptingLanguageTokenTypes>
{
    private Tokenizer<ScriptingLanguageTokenTypes> _tokenizer;

    protected override ITokenizer<ScriptingLanguageTokenTypes> Tokenizer => _tokenizer;

    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Definitions);
    }

    [Test]
    public void TestFun()
    {
        RunTest("fun", ScriptingLanguageTokenTypes.Fun);
    }

    [Test]
    public void TestFunWithMoreData()
    {
        RunTest("fun", (ExpectedToken<ScriptingLanguageTokenTypes>)ScriptingLanguageTokenTypes.Fun);
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithoutMoreData()
    {
        RunTest("fun ", 
        [
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Fun, MoreDataAvailable: true),
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " ")
        ]);
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithMoreData()
    {
        RunTest("fun ",
        [
            ScriptingLanguageTokenTypes.Fun,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " ")
        ], true);
    }

    [Test]
    public void TestFunWithOpenClosedParen()
    {
        RunTest("fun()",
        [
            ScriptingLanguageTokenTypes.Fun,
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
        ]);
    }

    [Test]
    public void TestFunWithSpaceThenTextThenOpenClosedParen()
    {
        RunTest("fun test()",
        [
            ScriptingLanguageTokenTypes.Fun,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
        ]);
    }

    [Test]
    public void TestFullFunctionDeclaration()
    {
        RunTest("fun test() { }",
        [
            ScriptingLanguageTokenTypes.Fun,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            ScriptingLanguageTokenTypes.OpenBrace,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            ScriptingLanguageTokenTypes.CloseBrace,
        ]);
    }

    [Test]
    public void TestFullFunctionDeclarationWithNewLines()
    {
        const string testStr = """
                               fun test() {

                               }
                               """;
        RunTest(testStr,
        [
            ScriptingLanguageTokenTypes.Fun,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            ScriptingLanguageTokenTypes.OpenBrace,
            new ExpectedToken<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, "\r\n\r\n"),
            ScriptingLanguageTokenTypes.CloseBrace,
        ]);
    }
}
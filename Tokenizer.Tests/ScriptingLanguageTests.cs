namespace Tokenizer.Tests;

public record ScriptingLanguageTokenTypes(string Lexeme, int Id) : TokenType<ScriptingLanguageTokenTypes>(Lexeme, Id), ITokenType<ScriptingLanguageTokenTypes>
{
    public static readonly ScriptingLanguageTokenTypes Fun = new("fun", 0);
    public static readonly ScriptingLanguageTokenTypes OpenParenthesis = Fun.Next("(");
    public static readonly ScriptingLanguageTokenTypes CloseParenthesis = OpenParenthesis.Next(")");
    public static readonly ScriptingLanguageTokenTypes OpenBrace = CloseParenthesis.Next("{");
    public static readonly ScriptingLanguageTokenTypes CloseBrace = OpenBrace.Next("}");

    public static IEnumerable<ScriptingLanguageTokenTypes> TokenTypes { get; } =
    [
        Fun,
        OpenParenthesis,
        CloseParenthesis,
        OpenBrace,
        CloseBrace
    ];

    public static ScriptingLanguageTokenTypes Create(string token, int id) => new(token, id);
}

public class ScriptingLanguageTests : TokenizerTestBase<ScriptingLanguageTokenTypes>
{
    [Test]
    public void TestFun()
    {
        RunTest("fun", ScriptingLanguageTokenTypes.Fun);
    }

    [Test]
    public void TestFunWithMoreData()
    {
        RunTest("fun", ScriptingLanguageTokenTypes.Fun);
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithoutMoreData()
    {
        RunTest("fun ", 
        [
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Fun, MoreDataAvailable: true),
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " ")
        ]);
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithMoreData()
    {
        RunTest("fun ",
        [
            ScriptingLanguageTokenTypes.Fun,
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " ", ExpectToParse: false) // False because there could be more whitespace to add to this token
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
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
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
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            ScriptingLanguageTokenTypes.OpenBrace,
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
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
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Text, "test"),
            ScriptingLanguageTokenTypes.OpenParenthesis,
            ScriptingLanguageTokenTypes.CloseParenthesis,
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, " "),
            ScriptingLanguageTokenTypes.OpenBrace,
            new TestCase<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.WhiteSpace, "\r\n\r\n"),
            ScriptingLanguageTokenTypes.CloseBrace,
        ]);
    }
}
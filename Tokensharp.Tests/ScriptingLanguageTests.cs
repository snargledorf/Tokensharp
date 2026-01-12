namespace Tokensharp.Tests;

public record ScriptingLanguageTokenTypes(string Identifier) : TokenType<ScriptingLanguageTokenTypes>(Identifier), ITokenType<ScriptingLanguageTokenTypes>
{
    public static readonly ScriptingLanguageTokenTypes Fun = new("fun");
    public static readonly ScriptingLanguageTokenTypes OpenParenthesis = new("(");
    public static readonly ScriptingLanguageTokenTypes CloseParenthesis = new(")");
    public static readonly ScriptingLanguageTokenTypes OpenBrace = new("{");
    public static readonly ScriptingLanguageTokenTypes CloseBrace = new("}");

    public static TokenConfiguration<ScriptingLanguageTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<ScriptingLanguageTokenTypes>
        {
            Fun,
            OpenParenthesis,
            CloseParenthesis,
            OpenBrace,
            CloseBrace
        }.Build();

    public static ScriptingLanguageTokenTypes Create(string token) => new(token);
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
namespace Tokenizer.Tests;

internal record ScriptingLanguageTokenTypes(string Lexeme, int Id) : TokenType<ScriptingLanguageTokenTypes>(Lexeme, Id), ITokenType<ScriptingLanguageTokenTypes>
{
    public static readonly ScriptingLanguageTokenTypes Fun = StartOfUserDefinedTokenTypes.Next("fun");
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

    public static ScriptingLanguageTokenTypes Maximum { get; } = Definitions.Last();
}

public class ScriptingLanguageTests
{
    private Tokenizer<ScriptingLanguageTokenTypes> _tokenizer;

    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<ScriptingLanguageTokenTypes>(ScriptingLanguageTokenTypes.Definitions);
    }

    [Test]
    public void TestFun()
    {
        bool parsed = _tokenizer.TryParseToken("fun", false, out ScriptingLanguageTokenTypes? token, out int tokenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True, "Failed to parse fun");
            Assert.That(token, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(tokenLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreData()
    {
        bool parsed = _tokenizer.TryParseToken("fun", true, out ScriptingLanguageTokenTypes? token, out int tokenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True, "Failed to parse fun");
            Assert.That(token, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(tokenLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithoutMoreData()
    {
        ReadOnlySpan<char> testStr = "fun ";

        bool parsedFun = _tokenizer.TryParseToken(testStr, true, out ScriptingLanguageTokenTypes? funToken, out int funTokenLength);

        testStr = testStr[funTokenLength..];

        bool parsedSpace =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? spaceToken, out int spaceTokenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funToken, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funTokenLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");

            Assert.That(parsedSpace, Is.True, "Failed to parse white space");
            Assert.That(spaceToken, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceTokenLength, Is.EqualTo(1), "White space length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithMoreData()
    {
        ReadOnlySpan<char> testStr = "fun ";

        bool parsedFun = _tokenizer.TryParseToken(testStr, true, out ScriptingLanguageTokenTypes? funToken, out int funTokenLength);

        testStr = testStr[funTokenLength..];

        bool parsedSpace =
            _tokenizer.TryParseToken(testStr, true, out ScriptingLanguageTokenTypes? spaceToken, out int spaceTokenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funToken, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funTokenLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");

            Assert.That(parsedSpace, Is.False, "Failed to parse white space");
            Assert.That(spaceToken, Is.Null);
            Assert.That(spaceTokenLength, Is.EqualTo(0), "White space length incorrect");
        });
    }

    [Test]
    public void TestFunWithOpenClosedParen()
    {
        ReadOnlySpan<char> testStr = "fun()";

        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? funTokenType, out int funLength);

        testStr = testStr[funLength..];

        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openParenTokenType,
            out int openParenLength);

        testStr = testStr[openParenLength..];

        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closedParenTokenType,
            out int closedParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");

            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis.Lexeme.Length),
                "Open paren length incorrect");

            Assert.That(parsedClosedParen, Is.True, "Failed to parse closed paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis.Lexeme.Length),
                "Closed paren length incorrect");
        });
    }

    [Test]
    public void TestFunWithSpaceThenTextThenOpenClosedParen()
    {
        ReadOnlySpan<char> testStr = "fun test()";

        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? funTokenType, out int funLength);

        testStr = testStr[funLength..];

        bool spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? spaceTokenType, out int spaceLength);

        testStr = testStr[spaceLength..];

        bool parsedText =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? textTokenType, out int textLength);

        testStr = testStr[textLength..];

        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openParenTokenType,
            out int openParenLength);

        testStr = testStr[openParenLength..];

        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closedParenTokenType,
            out int closedParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");

            Assert.That(spaceParsed, Is.True, "Failed to parse space");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space length incorrect");

            Assert.That(parsedText, Is.True, "Failed to parse text");
            Assert.That(textTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Text));
            Assert.That(textLength, Is.EqualTo(4), "Text length incorrect");

            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis.Lexeme.Length),
                "Open paren length incorrect");

            Assert.That(parsedClosedParen, Is.True, "Failed to parse closed paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis.Lexeme.Length),
                "Closed paren length incorrect");
        });
    }

    [Test]
    public void TestFullFunctionDeclaration()
    {
        ReadOnlySpan<char> testStr = "fun test() { }";

        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? funTokenType, out int funLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");
        });

        testStr = testStr[funLength..];

        bool spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? spaceTokenType, out int spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space before text");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedText =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? textTokenType, out int textLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedText, Is.True, "Failed to parse text");
            Assert.That(textTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Text));
            Assert.That(textLength, Is.EqualTo(4), "Text length incorrect");
        });

        testStr = testStr[textLength..];

        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openParenTokenType,
            out int openParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis.Lexeme.Length),
                "Open paren length incorrect");
        });

        testStr = testStr[openParenLength..];

        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closedParenTokenType,
            out int closedParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedClosedParen, Is.True, "Failed to parse close paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis.Lexeme.Length),
                "Close paren length incorrect");
        });

        testStr = testStr[closedParenLength..];

        spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out spaceTokenType, out spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space before open brace");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedOpenBrace = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openBraceTokenType,
            out int openBraceLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedOpenBrace, Is.True, "Failed to parse open brace");
            Assert.That(openBraceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenBrace));
            Assert.That(openBraceLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenBrace.Lexeme.Length),
                "Open brace length incorrect");
        });

        testStr = testStr[openBraceLength..];

        spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out spaceTokenType, out spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space after open brace");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedCloseBrace = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closeBraceTokenType,
            out int closeBraceLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedCloseBrace, Is.True, "Failed to parse close brace");
            Assert.That(closeBraceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseBrace));
            Assert.That(closeBraceLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseBrace.Lexeme.Length),
                "Close brace length incorrect");
        });
    }

    [Test]
    public void TestFullFunctionDeclarationWithNewLines()
    {
        ReadOnlySpan<char> testStr = """
                                     fun test() {
                                     
                                     }
                                     """;

        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? funTokenType, out int funLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Fun));
            Assert.That(funLength, Is.EqualTo(ScriptingLanguageTokenTypes.Fun.Lexeme.Length), "Fun length incorrect");
        });

        testStr = testStr[funLength..];

        bool spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? spaceTokenType, out int spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space before text");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space between fun and text length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedText =
            _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? textTokenType, out int textLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedText, Is.True, "Failed to parse text");
            Assert.That(textTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.Text));
            Assert.That(textLength, Is.EqualTo(4), "Text length incorrect");
        });

        testStr = testStr[textLength..];

        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openParenTokenType,
            out int openParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenParenthesis.Lexeme.Length),
                "Open paren length incorrect");
        });

        testStr = testStr[openParenLength..];

        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closedParenTokenType,
            out int closedParenLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedClosedParen, Is.True, "Failed to parse close paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseParenthesis.Lexeme.Length),
                "Close paren length incorrect");
        });

        testStr = testStr[closedParenLength..];

        spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out spaceTokenType, out spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space between close paren and open brace");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space between close paren and open brace length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedOpenBrace = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? openBraceTokenType,
            out int openBraceLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedOpenBrace, Is.True, "Failed to parse open brace");
            Assert.That(openBraceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.OpenBrace));
            Assert.That(openBraceLength, Is.EqualTo(ScriptingLanguageTokenTypes.OpenBrace.Lexeme.Length),
                "Open brace length incorrect");
        });

        testStr = testStr[openBraceLength..];

        spaceParsed =
            _tokenizer.TryParseToken(testStr, false, out spaceTokenType, out spaceLength);

        Assert.Multiple(() =>
        {
            Assert.That(spaceParsed, Is.True, "Failed to parse space after open brace");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(Environment.NewLine.Length * 2), "Space between braces length incorrect");
        });

        testStr = testStr[spaceLength..];

        bool parsedCloseBrace = _tokenizer.TryParseToken(testStr, false, out ScriptingLanguageTokenTypes? closeBraceTokenType,
            out int closeBraceLength);

        Assert.Multiple(() =>
        {
            Assert.That(parsedCloseBrace, Is.True, "Failed to parse close brace");
            Assert.That(closeBraceTokenType, Is.Not.Null.And.EqualTo(ScriptingLanguageTokenTypes.CloseBrace));
            Assert.That(closeBraceLength, Is.EqualTo(ScriptingLanguageTokenTypes.CloseBrace.Lexeme.Length),
                "Close brace length incorrect");
        });
    }
}
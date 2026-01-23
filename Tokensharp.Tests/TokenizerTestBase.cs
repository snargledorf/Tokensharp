
namespace Tokensharp.Tests;

public abstract class TokenizerTestBase<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private TokenConfiguration<TTokenType> _tokenConfiguration;

    [OneTimeSetUp]
    public void Setup()
    {
        _tokenConfiguration = TTokenType.Configuration;
    }
    
    protected void RunTest(string input,
        IEnumerable<TestCase<TTokenType>> testCases,
        bool moreDataAvailable = false)
    {
        RunTest(input.AsMemory(), testCases, moreDataAvailable);
    }

    protected void RunTest(ReadOnlyMemory<char> text,
        IEnumerable<TestCase<TTokenType>> testCases,
        bool moreDataAvailable = false)
    {
        foreach (TestCase<TTokenType> expectedToken in testCases)
            text = text[RunTest(text, expectedToken, moreDataAvailable)..];
        
        var tokenParser =  new TokenParser<TTokenType>(_tokenConfiguration);
        bool parsed = tokenParser.TryParseToken(text.Span, moreDataAvailable, out TokenType<TTokenType>? tokenType, out int length);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.False, "Parsed additional tokens");
            Assert.That(tokenType, Is.Null);
            Assert.That(length, Is.Zero);
        }
    }
    
    protected int RunTest(string input, TestCase<TTokenType> testCase, bool moreDataAvailable = false) =>
        RunTest(input.AsMemory(), testCase, moreDataAvailable);

    protected int RunTest(ReadOnlyMemory<char> text, TestCase<TTokenType> testCase, bool moreDataAvailable = false)
    {
        return RunTest(
            text, 
            testCase.TokenType, 
            testCase.Lexeme, 
            testCase.MoreDataAvailable || moreDataAvailable,
            testCase.ExpectToParse);
    }


    protected int RunTest(
        string text, 
        TTokenType expectedTokenType,
        string? expectedLexeme = null,
        bool moreDataAvailable = false, 
        bool expectToParse = true)
    {
        return RunTest(text.AsMemory(), expectedTokenType, expectedLexeme, moreDataAvailable, expectToParse);
    }

    protected int RunTest(ReadOnlyMemory<char> text, TTokenType expectedTokenType,
        string? expectedLexeme = null, bool moreDataAvailable = false, bool expectToParse = true)
    {
        if (expectedLexeme is null)
            expectedLexeme = expectedTokenType.Identifier;

        var tokenParser =  new TokenParser<TTokenType>(_tokenConfiguration);
        bool parsed = tokenParser.TryParseToken(text.Span, moreDataAvailable, out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme);

        using (Assert.EnterMultipleScope())
        {
            if (expectToParse)
            {
                Assert.That(parsed, Is.True);
                Assert.That(tokenType, Is.Not.Null.And.EqualTo(expectedTokenType));
                Assert.That(lexeme.ToString(), Is.EqualTo(expectedLexeme));
                Assert.That(lexeme.Length, Is.EqualTo(expectedLexeme.Length));
            }
            else
            {
                Assert.That(parsed, Is.False);
                Assert.That(tokenType, Is.Null);
                Assert.That(lexeme.IsEmpty, Is.True);
                Assert.That(lexeme.Length, Is.Zero);
            }
        }

        return lexeme.Length;
    }

    protected void RunTestShouldThrow<TExceptionType>(ReadOnlyMemory<char> text, Action<TExceptionType> validateException, bool moreDataAvailable = false) 
        where TExceptionType : Exception
    {
        var exception = Assert.Throws<TExceptionType>(() =>
        {
            while (Tokenizer.TryParseToken(text, _tokenConfiguration, moreDataAvailable, out Token<TTokenType> token))
                text = text[token.Lexeme.Length..];
        });
        
        if (exception is not null)
            validateException(exception);
    }
}
namespace Tokenizer.Tests;

public abstract class TokenizerTestBase<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected abstract ITokenizer<TTokenType> Tokenizer { get; }
    protected void RunTest(ReadOnlySpan<char> text,
        IEnumerable<TestCase<TTokenType>> expectedTokens,
        bool moreDataAvailable = false)
    {
        foreach (TestCase<TTokenType> expectedToken in expectedTokens)
            text = text[RunTest(text, expectedToken, moreDataAvailable)..];
    }

    protected int RunTest(ReadOnlySpan<char> text, TestCase<TTokenType> testCase, bool moreDataAvailable = false) =>
        RunTest(text, testCase.TokenType, testCase.Lexeme, testCase.MoreDataAvailable || moreDataAvailable, testCase.ExpectToParse);

    protected int RunTest(ReadOnlySpan<char> text, TTokenType expectedTokenType, string? lexeme = null, bool moreDataAvailable = false, bool expectToParse = true)
    {
        if (lexeme is null)
            lexeme = expectedTokenType.Lexeme;

        bool parsed =
            Tokenizer.TryParseToken(text, moreDataAvailable, out TTokenType? tokenType, out int tokenLength);

        var parsedLexeme = text[..tokenLength].ToString();

        Assert.Multiple(() =>
        {
            if (expectToParse)
            {
                Assert.That(parsed, Is.True);
                Assert.That(tokenType, Is.Not.Null.And.EqualTo(expectedTokenType));
                Assert.That(parsedLexeme, Is.EqualTo(lexeme));
            }
            else
            {
                Assert.That(parsed, Is.False);
                Assert.That(tokenType, Is.Null);
                Assert.That(parsedLexeme, Is.EqualTo(""));
            }
        });

        return tokenLength;
    }

    protected void RunTestShouldThrow<TExceptionType>(ReadOnlyMemory<char> text, Action<TExceptionType> validateException, bool moreDataAvailable = false) 
        where TExceptionType : Exception
    {
        var exception = Assert.Throws<TExceptionType>(() =>
        {
            while (Tokenizer.TryParseToken(text.Span, moreDataAvailable, out _, out int tokenLength))
                text = text[tokenLength..];
        });
        
        if (exception is not null)
            validateException(exception);
    }
}
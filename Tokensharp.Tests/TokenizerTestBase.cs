namespace Tokensharp.Tests;

public abstract class TokenizerTestBase<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
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
        
        bool parsed =
            Tokenizer.TryParseToken(text, moreDataAvailable, out Token<TTokenType> token);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.False, "Parsed additional tokens");
            Assert.That(token.Type, Is.Null);
            Assert.That(token.Lexeme, Is.Null);
        });
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

        bool parsed =
            Tokenizer.TryParseToken(text, moreDataAvailable, out Token<TTokenType> token);

        using (Assert.EnterMultipleScope())
        {
            if (expectToParse)
            {
                Assert.That(parsed, Is.True);
                Assert.That(token.Type, Is.Not.Null.And.EqualTo(expectedTokenType));
                Assert.That(token.Lexeme, Is.EqualTo(expectedLexeme));
                Assert.That(token.Length, Is.EqualTo(expectedLexeme.Length));
            }
            else
            {
                Assert.That(parsed, Is.False);
                Assert.That(token.Type, Is.Null);
                Assert.That(token.Lexeme, Is.Null);
                Assert.That(token.Length, Is.Zero);
            }
        }

        return token.Length;
    }

    protected void RunTestShouldThrow<TExceptionType>(ReadOnlyMemory<char> text, Action<TExceptionType> validateException, bool moreDataAvailable = false) 
        where TExceptionType : Exception
    {
        var exception = Assert.Throws<TExceptionType>(() =>
        {
            while (Tokenizer.TryParseToken(text, moreDataAvailable, out Token<TTokenType> token))
                text = text[token.Lexeme.Length..];
        });
        
        if (exception is not null)
            validateException(exception);
    }
}
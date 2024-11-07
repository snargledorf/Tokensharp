namespace Tokenizer.Tests;

public abstract class TokenizerTestBase<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected abstract ITokenizer<TTokenType> Tokenizer { get; }
    protected void RunTest(ReadOnlySpan<char> text,
        IEnumerable<ExpectedToken<TTokenType>> expectedTokens,
        bool moreDataAvailable = false)
    {
        foreach (ExpectedToken<TTokenType> expectedToken in expectedTokens)
            text = text[RunTest(text, expectedToken, moreDataAvailable)..];
    }

    protected int RunTest(ReadOnlySpan<char> text, ExpectedToken<TTokenType> expectedToken, bool moreDataAvailable = false) =>
        RunTest(text, expectedToken.TokenType, expectedToken.Lexeme, expectedToken.MoreDataAvailable || moreDataAvailable, expectedToken.ExpectToParse);

    protected int RunTest(ReadOnlySpan<char> text, TTokenType expectedTokenType, string? lexeme = null, bool moreDataAvailable = false, bool expectToParse = true)
    {
        if (lexeme is null)
            lexeme = expectedTokenType.Lexeme;
        
        bool parsed = Tokenizer.TryParseToken(text, moreDataAvailable, out TTokenType? tokenType, out int tokenLength);

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
}

public record ExpectedToken<TTokenType>(TTokenType TokenType, string? Lexeme = null, bool MoreDataAvailable = false, bool ExpectToParse = true)
{
    public static implicit operator ExpectedToken<TTokenType>(TTokenType tokenType) => new(tokenType);
}
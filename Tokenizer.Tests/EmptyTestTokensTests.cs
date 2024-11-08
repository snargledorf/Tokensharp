namespace Tokenizer.Tests;

public record EmptyTestTokens(string Lexeme) : TokenType<EmptyTestTokens>(Lexeme), ITokenType<EmptyTestTokens>
{
    public static EmptyTestTokens Create(string lexeme) => new(lexeme);

    public static IEnumerable<EmptyTestTokens> TokenTypes { get; } = [];
}

public record DuplicateLexemeTokens(string Lexeme) : TokenType<DuplicateLexemeTokens>(Lexeme), ITokenType<DuplicateLexemeTokens>
{
    public static DuplicateLexemeTokens Create(string lexeme) => new(lexeme);

    public static IEnumerable<DuplicateLexemeTokens> TokenTypes { get; } = [
        new("lexeme"),
        new("lexeme")
    ];
}

public class EmptyTestTokensTests : TokenizerTestBase<EmptyTestTokens>
{
    [Test]
    public void Number()
    {
        RunTest("1", EmptyTestTokens.Number, lexeme: "1");
    }
}

public class DuplicateTokensTests
{
    [Test]
    public void DuplicateLexeme()
    {
        Assert.Throws<DuplicateTokenLexemeException<DuplicateLexemeTokens>>(() => _ = new Tokenizer<DuplicateLexemeTokens>());
    }
}
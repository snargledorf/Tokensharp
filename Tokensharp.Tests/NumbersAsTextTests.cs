namespace Tokensharp.Tests;

public record NumbersAsTextTokens(string Identifier) : TokenType<NumbersAsTextTokens>(Identifier), ITokenType<NumbersAsTextTokens>
{
    public static NumbersAsTextTokens Create(string identifier) => new(identifier);

    public static TokenConfiguration<NumbersAsTextTokens> Configuration { get; } =
        new TokenConfigurationBuilder<NumbersAsTextTokens> { NumbersAreText = true }.Build();
}

public class NumbersAsTextTests : TokenizerTestBase<NumbersAsTextTokens>
{
    [Test]
    public void NumberIsTreatedAsText()
    {
        RunTest("123", TokenType<NumbersAsTextTokens>.Text, "123");
    }

    [Test]
    public void NumberFollowedByTextIsSingleToken()
    {
        RunTest("1a", TokenType<NumbersAsTextTokens>.Text, "1a");
    }

    [Test]
    public void TextFollowedByNumberIsSingleToken()
    {
        RunTest("a1", TokenType<NumbersAsTextTokens>.Text, "a1");
    }

    [Test]
    public void TextNumberTextIsSingleToken()
    {
        RunTest("a1b", TokenType<NumbersAsTextTokens>.Text, "a1b");
    }

    [Test]
    public void SeperateTokens()
    {
        RunTest("1 a",
        [
            new TestCase<NumbersAsTextTokens>(TokenType<NumbersAsTextTokens>.Text, "1"),
            new TestCase<NumbersAsTextTokens>(TokenType<NumbersAsTextTokens>.WhiteSpace, " "),
            new TestCase<NumbersAsTextTokens>(TokenType<NumbersAsTextTokens>.Text, "a")
        ]);
    }
}

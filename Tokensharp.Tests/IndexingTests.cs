using NUnit.Framework;

namespace Tokensharp.Tests;

public record IndexingTokens(string Identifier) : TokenType<IndexingTokens>(Identifier), ITokenType<IndexingTokens>
{
    public static readonly IndexingTokens First = Create("first");
    public static readonly IndexingTokens Second = Create("second");

    public static IndexingTokens Create(string identifier) => new(identifier);

    public static TokenConfiguration<IndexingTokens> Configuration { get; } = new TokenConfigurationBuilder<IndexingTokens>().Build();
}

public record OtherIndexingTokens(string Identifier) : TokenType<OtherIndexingTokens>(Identifier), ITokenType<OtherIndexingTokens>
{
    public static readonly OtherIndexingTokens A = Create("a");
    public static readonly OtherIndexingTokens B = Create("b");
    public static readonly OtherIndexingTokens C = Create("c");

    public static OtherIndexingTokens Create(string identifier) => new(identifier);

    public static TokenConfiguration<OtherIndexingTokens> Configuration { get; } = new TokenConfigurationBuilder<OtherIndexingTokens>().Build();
}

public class IndexingTests
{
    [Test]
    public void VerifyIndexingOrder()
    {
        // Base class defines Text, Number, WhiteSpace
        // They should have indices 0, 1, 2
        Assert.That((int)IndexingTokens.Text, Is.EqualTo(0));
        Assert.That((int)IndexingTokens.Number, Is.EqualTo(1));
        Assert.That((int)IndexingTokens.WhiteSpace, Is.EqualTo(2));

        // IndexingTokens defines First, Second
        // They should have indices 3, 4
        Assert.That((int)IndexingTokens.First, Is.EqualTo(3));
        Assert.That((int)IndexingTokens.Second, Is.EqualTo(4));
    }

    [Test]
    public void VerifyIndependentIndexing()
    {
        // OtherIndexingTokens should have its own counter starting at 0
        Assert.That((int)OtherIndexingTokens.Text, Is.EqualTo(0));
        Assert.That((int)OtherIndexingTokens.Number, Is.EqualTo(1));
        Assert.That((int)OtherIndexingTokens.WhiteSpace, Is.EqualTo(2));
        Assert.That((int)OtherIndexingTokens.A, Is.EqualTo(3));
        Assert.That((int)OtherIndexingTokens.B, Is.EqualTo(4));
        Assert.That((int)OtherIndexingTokens.C, Is.EqualTo(5));
    }
}

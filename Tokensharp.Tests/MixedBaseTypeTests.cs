namespace Tokensharp.Tests;

public record MixedBaseTypeTokenTypes(string Identifier)
    : TokenType<MixedBaseTypeTokenTypes>(Identifier), ITokenType<MixedBaseTypeTokenTypes>
{
    public static readonly MixedBaseTypeTokenTypes Mixed123 = new("ABC123");
    public static readonly MixedBaseTypeTokenTypes MixedWithSpace = new("ABC 123");
    public static readonly MixedBaseTypeTokenTypes StartsWithNumber = new("123ABC");
    public static readonly MixedBaseTypeTokenTypes DoubleMixed = new("ABC123DEF456");

    public static TokenConfiguration<MixedBaseTypeTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<MixedBaseTypeTokenTypes>
        {
            Mixed123,
            MixedWithSpace,
            StartsWithNumber,
            DoubleMixed
        }.Build();
    
    public static MixedBaseTypeTokenTypes Create(string token) => new(token);
}

public class MixedBaseTypeTests : TokenizerTestBase<MixedBaseTypeTokenTypes>
{
    [Test]
    public void TestCompleteMatch_Mixed123()
    {
        RunTest("ABC123", MixedBaseTypeTokenTypes.Mixed123);
    }

    [Test]
    public void TestIncompleteMatch_Mixed123_FallbackAtNumber()
    {
        // ABC12X -> ABC (Text), 12 (Number), X (Text)
        // 12 matches the '12' part of ABC123, but X fails.
        // So 12 should be consumed as Number, then X as Text.
        RunTest("ABC12X", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "ABC"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "12"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "X")
        ]);
    }

    [Test]
    public void TestIncompleteMatch_Mixed123_FallbackAtText()
    {
        // ABX123 -> AB (Text), X (Text), 123 (Number)
        // AB matches the 'AB' part of ABC123, but X fails.
        // So AB should be consumed as Text, then X as Text.
        RunTest("ABX123", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "ABX"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "123")
        ]);
    }

    [Test]
    public void TestIncompleteMatch_StartsWithNumber_FallbackAtText()
    {
        // 123ABX -> 123 (Number), AB (Text), X (Text)
        // AB matches 'AB' part of 123ABC, but X fails.
        RunTest("123ABX", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "123"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "ABX")
        ]);
    }

    [Test]
    public void TestIncompleteMatch_StartsWithNumber_FallbackAtNumber()
    {
        // 12XABC -> 12 (Number), X (Text), ABC (Text)
        // 12 matches '12' part of 123ABC, but X fails.
        RunTest("12XABC", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "12"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "XABC")
        ]);
    }

    [Test]
    public void TestIncompleteMatch_MixedWithSpace_FallbackAtNumber()
    {
        // ABC 12X -> ABC (Text), " " (WhiteSpace), 12 (Number), X (Text)
        RunTest("ABC 12X", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "ABC"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.WhiteSpace, " "),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "12"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "X")
        ]);
    }

    [Test]
    public void TestIncompleteMatch_DoubleMixed_FallbackAtSecondNumber()
    {
        // ABC123DEF45X -> ABC123 (Matches Mixed123), DEF (Text), 45 (Number), X (Text)
        // Mixed123 is a valid token, so it should be returned instead of base types.
        RunTest("ABC123DEF45X", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Mixed123),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "DEF"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "45"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "X")
        ]);
    }

    [Test]
    public void TestPartialMatch_MoreDataAvailable_ShouldWaitAcrossBoundaries()
    {
        // ABC12 with more data should wait to see if it becomes ABC123
        RunTest("ABC12", 
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Mixed123, ExpectToParse: false), 
            moreDataAvailable: true);
    }
    
    [Test]
    public void TestPartialMatch_NoMoreDataAvailable_ShouldFallbackAcrossBoundaries()
    {
        // ABC12 with no more data should fallback to base types
        RunTest("ABC12", [
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Text, "ABC"),
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.Number, "12")
        ], moreDataAvailable: false);
    }

    [Test]
    public void TestPartialMatch_MoreDataAvailable_StartsAtBoundary()
    {
        // 123 with more data should wait to see if it becomes 123ABC
        RunTest("123", 
            new TestCase<MixedBaseTypeTokenTypes>(MixedBaseTypeTokenTypes.StartsWithNumber, ExpectToParse: false), 
            moreDataAvailable: true);
    }
}

namespace Tokensharp.Tests;

public record LongestMatchTokenTypes(string Identifier) : TokenType<LongestMatchTokenTypes>(Identifier), ITokenType<LongestMatchTokenTypes>
{
    public static readonly LongestMatchTokenTypes Foo = new("foo");
    public static readonly LongestMatchTokenTypes Foobar = new("foobar");

    public static TokenConfiguration<LongestMatchTokenTypes> Configuration { get; } =
    [
        Foo,
        Foobar
    ];
    
    public static LongestMatchTokenTypes Create(string token) => new(token);
}

public class LongestMatchTests : TokenizerTestBase<LongestMatchTokenTypes>
{
    [Test]
    public void TestLongestMatch_Foobar()
    {
        // Should match "foobar" as a single token, not "foo" followed by "bar"
        RunTest("foobar", LongestMatchTokenTypes.Foobar);
    }

    [Test]
    public void TestLongestMatch_Foo()
    {
        RunTest("foo", LongestMatchTokenTypes.Foo);
    }
    
    [Test]
    public void TestLongestMatch_Foo_WithSuffix()
    {
        // "foob" -> "foo" (match) + "b" (remaining)
        // Since "b" is not a valid token start, the next call might fail or return text if configured.
        // But here we just check the first token.
        
        RunTest("foob", [
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Foo),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Text, "b")
        ]);
    }

    [Test]
    public void TestLongestMatch_Foobac()
    {
        RunTest("foobac", [
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Foo),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Text, "bac")
        ]);
    }

    [Test]
    public void TestLongestMatch_FoobacWithSpace()
    {
        RunTest("foobac ", [
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Foo),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Text, "bac"),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.WhiteSpace, " ")
        ]);
    }

    [Test]
    public void TestLongestMatch_TextFollowedByFoo()
    {
        RunTest("ThisIsTextfoo", [
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Text, "ThisIsText"),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Foo)
        ]);
    }

    [Test]
    public void TestLongestMatch_TextFollowedByFop()
    {
        RunTest("ThisIsTextfop", LongestMatchTokenTypes.Text, "ThisIsTextfop");
    }

    [Test]
    public void TestLongestMatch_IncompleteFoo()
    {
        RunTest("fo", LongestMatchTokenTypes.Text, "fo");
    }

    [Test]
    public void TestLongestMatch_IncompleteFooWithSpace()
    {
        RunTest("fo ", [
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.Text, "fo"),
            new TestCase<LongestMatchTokenTypes>(LongestMatchTokenTypes.WhiteSpace, " ")
        ]);
    }
}

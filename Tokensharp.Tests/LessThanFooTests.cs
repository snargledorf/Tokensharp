using Tokensharp.StateMachine;

namespace Tokensharp.Tests;

public record LessThanFooTokenTypes(string Identifier)
    : TokenType<LessThanFooTokenTypes>(Identifier), ITokenType<LessThanFooTokenTypes>
{
    public static readonly LessThanFooTokenTypes LessThanFoo = new("<Foo");
    public static readonly LessThanFooTokenTypes LessThanFooB = new("<FooB");
    public static readonly LessThanFooTokenTypes LessThanFooBar = new("<FooBar");

    public static TokenConfiguration<LessThanFooTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<LessThanFooTokenTypes>
        {
            LessThanFoo,
            LessThanFooB,
            LessThanFooBar
        }.Build();
    
    public static LessThanFooTokenTypes Create(string token) => new(token);
}

public class LessThanFooTests : TokenizerTestBase<LessThanFooTokenTypes>
{
    [Test]
    public void TestLongestMatch_LessThanFoo()
    {
        RunTest("123<Foo <FooB456<Foo789<FooB <FooABC<FooBar123<Foo <FooB456<Foo789<FooB <FooABC", [
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "123"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.WhiteSpace, " "),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFooB),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "456"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "789"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFooB),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.WhiteSpace, " "),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Text, "ABC"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFooBar),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "123"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.WhiteSpace, " "),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFooB),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "456"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Number, "789"),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFooB),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.WhiteSpace, " "),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.LessThanFoo),
            new TestCase<LessThanFooTokenTypes>(LessThanFooTokenTypes.Text, "ABC")
        ]);
    }
}

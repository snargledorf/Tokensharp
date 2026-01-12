using Tokensharp.StateMachine;

namespace Tokensharp.Tests;

public record SimpleTokenTypes(string Identifier) : TokenType<SimpleTokenTypes>(Identifier), ITokenType<SimpleTokenTypes>
{
    public static readonly SimpleTokenTypes A = new("A");
    public static readonly SimpleTokenTypes AB = new("AB");

    public static TokenConfiguration<SimpleTokenTypes> Configuration { get; } = [A, AB];
    
    public static SimpleTokenTypes Create(string token) => new(token);
}

public class TokenReaderTests : TokenizerTestBase<SimpleTokenTypes>
{
    [Test]
    public void TestMoreDataAvailableLogic_ConsumedShouldBeZero_WhenReturningFalse()
    {
        // "A" is a valid token, but "AB" is also a valid token.
        // If we have "A" and moreDataAvailable is true, we should wait because "B" might come next.
        
        string input = "A";
        var memory = input.AsMemory();
        
        TokenReaderStateMachine<SimpleTokenTypes> tokenReaderStateMachine = TokenReaderStateMachine<SimpleTokenTypes>.For(SimpleTokenTypes.Configuration);
        
        var tokenReader = new TokenReader<SimpleTokenTypes>(memory.Span, tokenReaderStateMachine, moreDataAvailable: true);
        bool result = tokenReader.Read(out TokenType<SimpleTokenTypes>? tokenType, out ReadOnlySpan<char> lexeme);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False, "Should return false because we are waiting for more data");
            Assert.That(tokenType, Is.Null);
            Assert.That(lexeme.Length, Is.Zero, "Lexeme should be empty if no token was read");
            Assert.That(tokenReader.Consumed, Is.Zero, "Consumed should be 0 if we decided to wait for more data");
        }
    }
}
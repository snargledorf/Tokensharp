using NUnit.Framework;
using Tokensharp;

namespace Tokensharp.Tests;

public record BugReproTokenTypes(string Identifier)
    : TokenType<BugReproTokenTypes>(Identifier), ITokenType<BugReproTokenTypes>
{
    public static readonly BugReproTokenTypes DoubleParen = new("((");
    public static readonly BugReproTokenTypes Plus = new("+");

    public static TokenConfiguration<BugReproTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<BugReproTokenTypes>
        {
            DoubleParen,
            Plus
        }.Build();
    
    public static BugReproTokenTypes Create(string token) => new(token);
}

[TestFixture]
public class BugReproTests
{
    [Test]
    public void TestMidParseBug()
    {
        // "(+" should match "(" as Text, then "+" as Plus.
        // If "((" is a token, the parser starts matching it at "(", 
        // then sees "+" and should realize it can't be "((".
        var parser = new TokenParser<BugReproTokenTypes>("(+", BugReproTokenTypes.Configuration);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(parser.Read(), Is.True, "First Read() should succeed");
            Assert.That(parser.TokenType, Is.EqualTo(TokenType<BugReproTokenTypes>.Text), "First token should be Text");
            Assert.That(parser.Lexeme.ToString(), Is.EqualTo("("));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parser.Read(), Is.True, "Second Read() should succeed");
            Assert.That(parser.TokenType, Is.EqualTo(BugReproTokenTypes.Plus), "Second token should be Plus");
            Assert.That(parser.Lexeme.ToString(), Is.EqualTo("+"));
        }
    }
}

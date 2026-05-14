using NUnit.Framework;
using Tokensharp;

namespace Tokensharp.Tests;

public record PartialTokenMatchTokenTypes(string Identifier)
    : TokenType<PartialTokenMatchTokenTypes>(Identifier), ITokenType<PartialTokenMatchTokenTypes>
{
    public static readonly PartialTokenMatchTokenTypes DoubleParen = new("((");
    public static readonly PartialTokenMatchTokenTypes Plus = new("+");

    public static TokenConfiguration<PartialTokenMatchTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<PartialTokenMatchTokenTypes>
        {
            DoubleParen,
            Plus
        }.Build();
    
    public static PartialTokenMatchTokenTypes Create(string token) => new(token);
}

[TestFixture]
public class PartialTokenMatchTests
{
    [Test]
    public void TestMidParseFallbackToText()
    {
        // "(+" should match "(" as Text, then "+" as Plus.
        // If "((" is a token, the parser starts matching it at "(", 
        // then sees "+" and should realize it can't be "((".
        var parser = new TokenParser<PartialTokenMatchTokenTypes>("(+", PartialTokenMatchTokenTypes.Configuration);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(parser.Read(), Is.True, "First Read() should succeed");
            Assert.That(parser.TokenType, Is.EqualTo(TokenType<PartialTokenMatchTokenTypes>.Text), "First token should be Text");
            Assert.That(parser.Lexeme.ToString(), Is.EqualTo("("));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parser.Read(), Is.True, "Second Read() should succeed");
            Assert.That(parser.TokenType, Is.EqualTo(PartialTokenMatchTokenTypes.Plus), "Second token should be Plus");
            Assert.That(parser.Lexeme.ToString(), Is.EqualTo("+"));
        }
    }
}

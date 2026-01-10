namespace Tokensharp.Tests;

public class TokenConfigurationBuilderTests
{
    [Test]
    public void Tokenizer_WorksWithBuiltConfiguration()
    {
        var builder = new TokenConfigurationBuilder();
        builder.AddTokenType("Foo");
        builder.AddTokenType("Bar");
        
        TokenConfiguration configuration = builder.Build();
        
        var tokens = Tokenizer.EnumerateTokens("Foo Bar", configuration).ToArray();
        
        Assert.That(tokens, Has.Length.EqualTo(3));
        Assert.That(tokens[0].Lexeme, Is.EqualTo("Foo"));
        Assert.That(tokens[0].Type.Lexeme, Is.EqualTo("Foo"));
        
        Assert.That(tokens[1].Type, Is.EqualTo(TokenType<RuntimeConfigToken>.WhiteSpace));
        
        Assert.That(tokens[2].Lexeme, Is.EqualTo("Bar"));
        Assert.That(tokens[2].Type.Lexeme, Is.EqualTo("Bar"));
    }

    [Test]
    public void Tokenizer_WorksWithEmptyConfiguration()
    {
        var builder = new TokenConfigurationBuilder();
        TokenConfiguration configuration = builder.Build();
        
        var tokens = Tokenizer.EnumerateTokens("Foo", configuration).ToArray();
        
        Assert.That(tokens, Has.Length.EqualTo(1));
        Assert.That(tokens[0].Type, Is.EqualTo(TokenType<RuntimeConfigToken>.Text));
        Assert.That(tokens[0].Lexeme, Is.EqualTo("Foo"));
    }
    
    [Test]
    public void Tokenizer_WorksWithDuplicateTokensAdded()
    {
        var builder = new TokenConfigurationBuilder();
        builder.AddTokenType("Foo");
        builder.AddTokenType("Foo");
        
        TokenConfiguration configuration = builder.Build();
        
        var tokens = Tokenizer.EnumerateTokens("Foo", configuration).ToArray();
        
        Assert.That(tokens, Has.Length.EqualTo(1));
        Assert.That(tokens[0].Lexeme, Is.EqualTo("Foo"));
        Assert.That(tokens[0].Type.Lexeme, Is.EqualTo("Foo"));
    }
}
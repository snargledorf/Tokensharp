namespace Tokensharp.Tests;

public class EnumerateTokensTests
{
    [Test]
    public void EnumerateSpan()
    {
        Token<EmptyTokens>[] tokens = Tokenizer.EnumerateTokens<EmptyTokens>("Hello World").ToArray();
        
        var expectedTokens = new[] 
        {
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "Hello"),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.WhiteSpace, " "),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "World"),
        };
        
        Assert.That(tokens, Is.EquivalentTo(expectedTokens));
    }

    [Test]
    public async Task EnumerateAsync()
    {
        await using var ms = new MemoryStream("Hello World"u8.ToArray());
        ms.Position = 0;

        Token<EmptyTokens>[] tokens = await Tokenizer.EnumerateTokensAsync<EmptyTokens>(ms).ToArrayAsync();

        var expectedTokens = new[] 
        {
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "Hello"),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.WhiteSpace, " "),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "World"),
        };
        
        Assert.That(tokens, Is.EquivalentTo(expectedTokens));
    }
    
    [Test]
    public void IgnoreWhiteSpace()
    {
        var tokenizerOptions = new TokenizerOptions { IgnoreWhiteSpace = true };
        
        Token<EmptyTokens>[] tokens = Tokenizer.EnumerateTokens<EmptyTokens>("Hello World", tokenizerOptions).ToArray();
        
        var expectedTokens = new[] 
        {
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "Hello"),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "World"),
        };
        
        Assert.That(tokens, Is.EquivalentTo(expectedTokens));
    }
    
    [Test]
    public async Task IgnoreWhiteSpaceAsync()
    {
        await using var ms = new MemoryStream("Hello World"u8.ToArray());
        ms.Position = 0;
        
        var tokenizerOptions = new TokenizerOptions { IgnoreWhiteSpace = true };
        
        Token<EmptyTokens>[] tokens = await Tokenizer.EnumerateTokensAsync<EmptyTokens>(ms, tokenizerOptions).ToArrayAsync();
        
        var expectedTokens = new[] 
        {
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "Hello"),
            new Token<EmptyTokens>(TokenType<EmptyTokens>.Text, "World"),
        };
        
        Assert.That(tokens, Is.EquivalentTo(expectedTokens));
    }
}
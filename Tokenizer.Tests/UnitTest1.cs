namespace Tokenizer.Tests;

public class Tests
{
    private Tokenizer<TestTokenType> _tokenizer;
    
    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<TestTokenType>(TestTokenType.Definitions);
    }

    [Test]
    public void TestFun()
    {
        bool parsed = _tokenizer.TryParseToken("fun", false, out TestTokenType? token, out int tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True, "Failed to parse fun");
            Assert.That(token, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(tokenLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreData()
    {
        bool parsed = _tokenizer.TryParseToken("fun", true, out TestTokenType? token, out int tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True, "Failed to parse fun");
            Assert.That(token, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(tokenLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithoutMoreData()
    {
        ReadOnlySpan<char> testStr = "fun ";
        
        bool parsedFun = _tokenizer.TryParseToken(testStr, true, out TestTokenType? funToken, out int funTokenLength);
        
        testStr = testStr[funTokenLength..];
        
        bool parsedSpace = _tokenizer.TryParseToken(testStr, false, out TestTokenType? spaceToken, out int spaceTokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funToken, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(funTokenLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
            
            Assert.That(parsedSpace, Is.True, "Failed to parse white space");
            Assert.That(spaceToken, Is.Not.Null.And.EqualTo(TestTokenType.WhiteSpace));
            Assert.That(spaceTokenLength, Is.EqualTo(1), "White space length incorrect");
        });
    }

    [Test]
    public void TestFunWithMoreDataAndSpaceWithMoreData()
    {
        ReadOnlySpan<char> testStr = "fun ";
        
        bool parsedFun = _tokenizer.TryParseToken(testStr, true, out TestTokenType? funToken, out int funTokenLength);
        
        testStr = testStr[funTokenLength..];
        
        bool parsedSpace = _tokenizer.TryParseToken(testStr, true, out TestTokenType? spaceToken, out int spaceTokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funToken, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(funTokenLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
            
            Assert.That(parsedSpace, Is.False, "Failed to parse white space");
            Assert.That(spaceToken, Is.Null);
            Assert.That(spaceTokenLength, Is.EqualTo(0), "White space length incorrect");
        });
    }

    [Test]
    public void TestFunWithOpenClosedParen()
    {
        ReadOnlySpan<char> testStr = "fun()";
        
        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out TestTokenType? funTokenType, out int funLength);
        
        testStr = testStr[funLength..];
        
        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out TestTokenType? openParenTokenType, out int openParenLength);
        
        testStr = testStr[openParenLength..];
        
        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out TestTokenType? closedParenTokenType, out int closedParenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(funLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
            
            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(TestTokenType.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(TestTokenType.OpenParenthesis.Lexeme.Length), "Open paren length incorrect");
            
            Assert.That(parsedClosedParen, Is.True, "Failed to parse closed paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(TestTokenType.ClosedParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(TestTokenType.ClosedParenthesis.Lexeme.Length), "Closed paren length incorrect");
        });
    }

    [Test]
    public void TestFunWithSpaceThenTextThenOpenClosedParen()
    {
        ReadOnlySpan<char> testStr = "fun test()";
        
        bool parsedFun = _tokenizer.TryParseToken(testStr, false, out TestTokenType? funTokenType, out int funLength);
        
        testStr = testStr[funLength..];
        
        bool spaceParsed = _tokenizer.TryParseToken(testStr, false, out TestTokenType? spaceTokenType, out int spaceLength);
        
        testStr = testStr[spaceLength..];
        
        bool parsedText = _tokenizer.TryParseToken(testStr, false, out TestTokenType? textTokenType, out int textLength);
        
        testStr = testStr[textLength..];
        
        bool parsedOpenParen = _tokenizer.TryParseToken(testStr, false, out TestTokenType? openParenTokenType, out int openParenLength);
        
        testStr = testStr[openParenLength..];
        
        bool parsedClosedParen = _tokenizer.TryParseToken(testStr, false, out TestTokenType? closedParenTokenType, out int closedParenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedFun, Is.True, "Failed to parse fun");
            Assert.That(funTokenType, Is.Not.Null.And.EqualTo(TestTokenType.Fun));
            Assert.That(funLength, Is.EqualTo(TestTokenType.Fun.Lexeme.Length), "Fun length incorrect");
            
            Assert.That(spaceParsed, Is.True, "Failed to parse space");
            Assert.That(spaceTokenType, Is.Not.Null.And.EqualTo(TestTokenType.WhiteSpace));
            Assert.That(spaceLength, Is.EqualTo(1), "Space length incorrect");
            
            Assert.That(parsedText, Is.True, "Failed to parse text");
            Assert.That(textTokenType, Is.Not.Null.And.EqualTo(TestTokenType.Text));
            Assert.That(textLength, Is.EqualTo(4), "Text length incorrect");
            
            Assert.That(parsedOpenParen, Is.True, "Failed to parse open paren");
            Assert.That(openParenTokenType, Is.Not.Null.And.EqualTo(TestTokenType.OpenParenthesis));
            Assert.That(openParenLength, Is.EqualTo(TestTokenType.OpenParenthesis.Lexeme.Length), "Open paren length incorrect");
            
            Assert.That(parsedClosedParen, Is.True, "Failed to parse closed paren");
            Assert.That(closedParenTokenType, Is.Not.Null.And.EqualTo(TestTokenType.ClosedParenthesis));
            Assert.That(closedParenLength, Is.EqualTo(TestTokenType.ClosedParenthesis.Lexeme.Length), "Closed paren length incorrect");
        });
    }
}
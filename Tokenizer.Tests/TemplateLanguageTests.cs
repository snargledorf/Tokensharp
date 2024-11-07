namespace Tokenizer.Tests;

internal record TemplateLanguageTokenTypes(string Lexeme, int Id) : TokenType<TemplateLanguageTokenTypes>(Lexeme, Id), ITokenType<TemplateLanguageTokenTypes>
{
    public static readonly TemplateLanguageTokenTypes StartBinding = StartOfUserDefinedTokenTypes.Next("{{");
    public static readonly TemplateLanguageTokenTypes EndBinding = StartBinding.Next("}}");

    public static readonly IEnumerable<TemplateLanguageTokenTypes> Definitions =
    [
        StartBinding,
        EndBinding,
    ];
    
    public static TemplateLanguageTokenTypes Create(string token, int id) => new(token, id);

    public static TemplateLanguageTokenTypes Maximum { get; } = Definitions.Last();
}

public class TemplateLanguageTests
{
    private Tokenizer<TemplateLanguageTokenTypes> _tokenizer;

    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.Definitions);
    }

    [Test]
    public void BasicBinding()
    {
        ReadOnlySpan<char> testStr = "{{text}}";

        bool parsedToken = _tokenizer.TryParseToken(testStr, false, out TemplateLanguageTokenTypes? tokenType, out int tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True);
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.StartBinding));
            Assert.That(tokenLength, Is.EqualTo(TemplateLanguageTokenTypes.StartBinding.Lexeme.Length));
        });
        
        testStr = testStr[tokenLength..];

        parsedToken = _tokenizer.TryParseToken(testStr, false, out tokenType, out tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True);
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.Text));
            Assert.That(tokenLength, Is.EqualTo(4));
        });
        
        testStr = testStr[tokenLength..];

        parsedToken = _tokenizer.TryParseToken(testStr, false, out tokenType, out tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True);
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.EndBinding));
            Assert.That(tokenLength, Is.EqualTo(TemplateLanguageTokenTypes.EndBinding.Lexeme.Length));
        });
    }

    [Test]
    public void TextHasPartialTokensWhichShouldBeReadAsText()
    {
        ReadOnlySpan<char> testStr = "{{{}text}}";

        bool parsedToken = _tokenizer.TryParseToken(testStr, false, out TemplateLanguageTokenTypes? tokenType, out int tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True, "Parsed start binding");
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.StartBinding));
            Assert.That(tokenLength, Is.EqualTo(TemplateLanguageTokenTypes.StartBinding.Lexeme.Length), "Start binding length");
        });
        
        testStr = testStr[tokenLength..];

        parsedToken = _tokenizer.TryParseToken(testStr, false, out tokenType, out tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True, "Parsed Text");
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.Text));
            Assert.That(tokenLength, Is.EqualTo(6), "Text length");
        });
        
        testStr = testStr[tokenLength..];

        parsedToken = _tokenizer.TryParseToken(testStr, false, out tokenType, out tokenLength);
        
        Assert.Multiple(() =>
        {
            Assert.That(parsedToken, Is.True, "Parsed end binding");
            Assert.That(tokenType, Is.Not.Null.And.EqualTo(TemplateLanguageTokenTypes.EndBinding));
            Assert.That(tokenLength, Is.EqualTo(TemplateLanguageTokenTypes.EndBinding.Lexeme.Length), "End binding length");
        });
    }
}
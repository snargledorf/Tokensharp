namespace Tokenizer.Tests;

public record TemplateLanguageTokenTypes(string Lexeme, int Id) : TokenType<TemplateLanguageTokenTypes>(Lexeme, Id), ITokenType<TemplateLanguageTokenTypes>
{
    public static readonly TemplateLanguageTokenTypes StartBinding = new("{{", 0);
    public static readonly TemplateLanguageTokenTypes EndBinding = StartBinding.Next("}}");

    public static IEnumerable<TemplateLanguageTokenTypes> TokenTypes { get; } =
    [
        StartBinding,
        EndBinding,
    ];
    
    public static TemplateLanguageTokenTypes Create(string token, int id) => new(token, id);
}

public class TemplateLanguageTests : TokenizerTestBase<TemplateLanguageTokenTypes>
{
    private Tokenizer<TemplateLanguageTokenTypes> _tokenizer;

    protected override ITokenizer<TemplateLanguageTokenTypes> Tokenizer => _tokenizer;

    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.TokenTypes);
    }

    [Test]
    public void BasicBinding()
    {
        ReadOnlySpan<char> testStr = "{{text}}";
        
        RunTest(testStr,
        [
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.StartBinding),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.Text, "text"),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.EndBinding),
        ]);
    }

    [Test]
    public void TextHasPartialTokensWhichShouldBeReadAsText()
    {
        ReadOnlySpan<char> testStr = "{{{}text}}";
        
        RunTest(testStr,
        [
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.StartBinding),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.Text, "{}text"),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.EndBinding),
        ]);
    }
}
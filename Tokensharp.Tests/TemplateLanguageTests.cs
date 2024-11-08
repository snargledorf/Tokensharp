namespace Tokensharp.Tests;

public record TemplateLanguageTokenTypes(string Lexeme) : TokenType<TemplateLanguageTokenTypes>(Lexeme), ITokenType<TemplateLanguageTokenTypes>
{
    public static readonly TemplateLanguageTokenTypes StartBinding = new("{{");
    public static readonly TemplateLanguageTokenTypes EndBinding = StartBinding.Next("}}");

    public static IEnumerable<TemplateLanguageTokenTypes> TokenTypes { get; } =
    [
        StartBinding,
        EndBinding,
    ];
    
    public static TemplateLanguageTokenTypes Create(string token) => new(token);
}

public class TemplateLanguageTests : TokenizerTestBase<TemplateLanguageTokenTypes>
{
    [Test]
    public void BasicBinding()
    {
        RunTest("{{text}}",
        [
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.StartBinding),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.Text, "text"),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.EndBinding),
        ]);
    }

    [Test]
    public void TextHasPartialTokensWhichShouldBeReadAsText()
    {
        RunTest("{{{}text}}",
        [
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.StartBinding),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.Text, "{}text"),
            new TestCase<TemplateLanguageTokenTypes>(TemplateLanguageTokenTypes.EndBinding),
        ]);
    }
}
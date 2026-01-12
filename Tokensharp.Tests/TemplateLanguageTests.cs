namespace Tokensharp.Tests;

public record TemplateLanguageTokenTypes(string Identifier) : TokenType<TemplateLanguageTokenTypes>(Identifier), ITokenType<TemplateLanguageTokenTypes>
{
    public static readonly TemplateLanguageTokenTypes StartBinding = new("{{");
    public static readonly TemplateLanguageTokenTypes EndBinding = new("}}");

    public static TokenConfiguration<TemplateLanguageTokenTypes> Configuration { get; } =
        new TokenConfigurationBuilder<TemplateLanguageTokenTypes>()
        {
            StartBinding,
            EndBinding,
        }.Build();
    
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
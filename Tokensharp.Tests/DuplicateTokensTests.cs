namespace Tokensharp.Tests;

public record DuplicateLexemeTokens(string Identifier)
    : TokenType<DuplicateLexemeTokens>(Identifier), ITokenType<DuplicateLexemeTokens>
{
    public static DuplicateLexemeTokens Create(string lexeme) => new(lexeme);

    public static TokenConfiguration<DuplicateLexemeTokens> Configuration { get; } = new TokenConfigurationBuilder<DuplicateLexemeTokens>()
    {
        { "lexeme", Create("lexeme") },
        { "lexeme", Create("lexeme") }
    }.Build();
}

public class DuplicateTokensTests
{
    [Test]
    public void DuplicateLexeme()
    {
        var typeInitException = Assert.Throws<TypeInitializationException>(() => _ = DuplicateLexemeTokens.Configuration);
        Assert.That(typeInitException, Has.InnerException.TypeOf<DuplicateLexemeException>());
        
        var duplicateLexemeException = (typeInitException.InnerException as DuplicateLexemeException);
        Assert.That(duplicateLexemeException?.Lexeme, Is.EqualTo("lexeme"));
    }
}
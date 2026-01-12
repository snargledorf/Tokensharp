namespace Tokensharp.Tests;

public record DuplicateLexemeTokens(string Identifier)
    : TokenType<DuplicateLexemeTokens>(Identifier), ITokenType<DuplicateLexemeTokens>
{
    public static DuplicateLexemeTokens Create(string lexeme) => new(lexeme);

    public static TokenConfiguration<DuplicateLexemeTokens> Configuration { get; } = new()
    {
        { "lexeme", Create("lexeme") },
        { "lexeme", Create("lexeme") }
    };
}

public class DuplicateTokensTests : TokenizerTestBase<DuplicateLexemeTokens>
{
    [Test]
    public void DuplicateLexeme()
    {
        RunTestShouldThrow<TypeInitializationException>(ReadOnlyMemory<char>.Empty, exception =>
        {
            Assert.That(exception, Has.InnerException.TypeOf<DuplicateLexemeException>());

            string? duplicateLexeme =
                (exception.InnerException as DuplicateLexemeException)
                ?.Lexeme;
            
            Assert.That(duplicateLexeme, Is.EqualTo("lexeme"));
        });
    }
}
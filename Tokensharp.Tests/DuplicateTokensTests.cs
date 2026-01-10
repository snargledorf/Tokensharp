namespace Tokensharp.Tests;

public record DuplicateLexemeTokens(string Lexeme) : TokenType<DuplicateLexemeTokens>(Lexeme), ITokenType<DuplicateLexemeTokens>
{
    public static DuplicateLexemeTokens Create(string lexeme) => new(lexeme);

    public static IEnumerable<DuplicateLexemeTokens> TokenTypes { get; } = [
        new("lexeme"),
        new("lexeme")
    ];
}

public class DuplicateTokensTests : TokenizerTestBase<DuplicateLexemeTokens>
{
    [Test]
    public void DuplicateLexeme()
    {
        RunTestShouldThrow<TypeInitializationException>(ReadOnlyMemory<char>.Empty, exception =>
        {
            Assert.That(exception, Has.InnerException.TypeOf<DuplicateTokenLexemeException<DuplicateLexemeTokens>>());

            DuplicateLexemeTokens[]? duplicateTokenTypes =
                (exception.InnerException as DuplicateTokenLexemeException<DuplicateLexemeTokens>)
                ?.TokenTypes.SelectMany(g => g).ToArray();
            
            Assert.That(duplicateTokenTypes, Has.Exactly(2).EqualTo(new DuplicateLexemeTokens("lexeme")));
        });
    }
}
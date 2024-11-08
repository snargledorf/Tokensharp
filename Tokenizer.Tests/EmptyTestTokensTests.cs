namespace Tokenizer.Tests;

public record EmptyTestTokens(string Lexeme, int Id) : TokenType<EmptyTestTokens>(Lexeme, Id), ITokenType<EmptyTestTokens>
{
    public static EmptyTestTokens Create(string lexeme, int id) => new(lexeme, id);

    public static IEnumerable<EmptyTestTokens> TokenTypes { get; } = [];
}

public record DuplicateIdTokens(string Lexeme, int Id) : TokenType<DuplicateIdTokens>(Lexeme, Id), ITokenType<DuplicateIdTokens>
{
    public static DuplicateIdTokens Create(string lexeme, int id) => new(lexeme, id);

    public static IEnumerable<DuplicateIdTokens> TokenTypes { get; } = [
        new("lexeme1", 0),
        new("lexeme2", 0)
    ];
}

public record DuplicateLexemeTokens(string Lexeme, int Id) : TokenType<DuplicateLexemeTokens>(Lexeme, Id), ITokenType<DuplicateLexemeTokens>
{
    public static DuplicateLexemeTokens Create(string lexeme, int id) => new(lexeme, id);

    public static IEnumerable<DuplicateLexemeTokens> TokenTypes { get; } = [
        new("lexeme", 0),
        new("lexeme", 1)
    ];
}

public class EmptyTestTokensTests : TokenizerTestBase<EmptyTestTokens>
{
    [Test]
    public void Number()
    {
        RunTest("1", EmptyTestTokens.Number, lexeme: "1");
    }
}

public class DuplicateTokensTests
{
    [Test]
    public void DuplicateIds()
    {
        Assert.Throws<DuplicateTokenIdException<DuplicateIdTokens>>(() => _ = new Tokenizer<DuplicateIdTokens>());
    }
    
    [Test]
    public void DuplicateLexeme()
    {
        Assert.Throws<DuplicateTokenLexemeException<DuplicateLexemeTokens>>(() => _ = new Tokenizer<DuplicateLexemeTokens>());
    }
}
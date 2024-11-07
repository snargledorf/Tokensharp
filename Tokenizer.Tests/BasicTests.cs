namespace Tokenizer.Tests;

public record BasicTestToken(string Lexeme, int Id) : TokenType<BasicTestToken>(Lexeme, Id), ITokenType<BasicTestToken>
{
    public static BasicTestToken Create(string lexeme, int id) => new(lexeme, id);

    public static IEnumerable<BasicTestToken> TokenTypes { get; } = [];
}

public class BasicTests : TokenizerTestBase<BasicTestToken>
{
    [Test]
    public void Number()
    {
        RunTest("1", BasicTestToken.Number, lexeme: "1");
    }
}
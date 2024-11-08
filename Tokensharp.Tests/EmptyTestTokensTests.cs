namespace Tokensharp.Tests;

public class EmptyTestTokensTests : TokenizerTestBase<EmptyTokens>
{
    [Test]
    public void Number()
    {
        RunTest("1", EmptyTokens.Number, lexeme: "1");
    }
}
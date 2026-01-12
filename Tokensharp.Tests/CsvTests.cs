namespace Tokensharp.Tests;

public record CsvTokenTypes(string Identifier) : TokenType<CsvTokenTypes>(Identifier), ITokenType<CsvTokenTypes>
{
    public static readonly CsvTokenTypes Comma = new(",");
    public static readonly CsvTokenTypes EndOfRecord = new("\r\n");
    public static readonly CsvTokenTypes DoubleQuote = new("\"");

    public static TokenConfiguration<CsvTokenTypes> Configuration { get; } = new TokenConfigurationBuilder<CsvTokenTypes>()
    {
        Comma,
        EndOfRecord,
        DoubleQuote
    }.Build();
    
    public static CsvTokenTypes Create(string token) => new(token);
}

public class CsvTests : TokenizerTestBase<CsvTokenTypes>
{
    [Test]
    public void CsvWithQuotesAndNewLine()
    {
        const string testStr = """
                               test, 123
                               foo,"bar
                               bizz"
                               """;

        RunTest(testStr, 
        [
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "test"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.WhiteSpace, " "),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Number, "123"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.EndOfRecord),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "foo"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "bar"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.EndOfRecord),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "bizz"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote)
        ]);
    }
}
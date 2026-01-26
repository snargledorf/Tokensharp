namespace Tokensharp.Tests;

public record CsvTokenTypes(string Identifier) : TokenType<CsvTokenTypes>(Identifier), ITokenType<CsvTokenTypes>
{
    public static readonly CsvTokenTypes Comma = new("comma");
    public static readonly CsvTokenTypes EndOfRecord = new("endOfRecord");
    public static readonly CsvTokenTypes DoubleQuote = new("doubleQuote");
    public static readonly CsvTokenTypes Escape = new("escape");

    public static TokenConfiguration<CsvTokenTypes> Configuration { get; } = new TokenConfigurationBuilder<CsvTokenTypes>(
    [
        new LexemeToTokenType<CsvTokenTypes>(",", Comma),
        new LexemeToTokenType<CsvTokenTypes>("\r\n", EndOfRecord),
        new LexemeToTokenType<CsvTokenTypes>("\"", DoubleQuote),
        new LexemeToTokenType<CsvTokenTypes>("\"\"", Escape),
    ]) { NumbersAreText = true }.Build();
    
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
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma, ","),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.WhiteSpace, " "),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "123"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.EndOfRecord, "\r\n"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "foo"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma, ","),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote, "\""),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "bar"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.EndOfRecord, "\r\n"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "bizz"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote, "\"")
        ]);
    }

    [Test]
    public void CsvWithEscapedQuotes()
    {   
        const string testStr = "123, \"\"\"Bar\"\"\" ,ABC";
        
        RunTest(testStr, 
        [
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "123"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma, ","),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.WhiteSpace, " "),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Escape, "\"\""),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote, "\""),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "Bar"),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Escape, "\"\""),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.DoubleQuote, "\""),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.WhiteSpace, " "),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Comma, ","),
            new TestCase<CsvTokenTypes>(CsvTokenTypes.Text, "ABC")
        ]);
    }
}
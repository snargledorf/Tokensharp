namespace Tokenizer.Tests;

public record CsvTokenTypes(string Lexeme, int Id) : TokenType<CsvTokenTypes>(Lexeme, Id), ITokenType<CsvTokenTypes>
{
    public static readonly CsvTokenTypes Comma = new(",", 0);
    public static readonly CsvTokenTypes EndOfRecord = Comma.Next("\r\n");
    public static readonly CsvTokenTypes DoubleQuote = EndOfRecord.Next("\"");

    public static readonly IEnumerable<CsvTokenTypes> Definitions =
    [
        Comma,
        EndOfRecord,
        DoubleQuote
    ];
    
    public static CsvTokenTypes Create(string token, int id) => new(token, id);
    public static CsvTokenTypes LastUserDefinedTokenType { get; } = Definitions.Last();
}

public class CsvTests : TokenizerTestBase<CsvTokenTypes>
{
    private Tokenizer<CsvTokenTypes> _tokenizer;

    protected override ITokenizer<CsvTokenTypes> Tokenizer => _tokenizer;

    [SetUp]
    public void Setup()
    {
        _tokenizer = new Tokenizer<CsvTokenTypes>(CsvTokenTypes.Definitions);
    }

    [Test]
    public void CsvWithQuotesAndNewLine()
    {
        ReadOnlySpan<char> testStr = """
                                     test, 123
                                     foo,"bar
                                     bizz"
                                     """;

        RunTest(testStr, 
        [
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Text, "test"),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Comma),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.WhiteSpace, " "),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Text, "123"),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.EndOfRecord),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Text, "foo"),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Comma),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.DoubleQuote),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Text, "bar"),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.EndOfRecord),
            new ExpectedToken<CsvTokenTypes>(CsvTokenTypes.Text, "bizz")
        ]);
    }
}
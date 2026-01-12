namespace Tokensharp.Benchmark;

public record CsvTokenTypes(string Identifier) : TokenType<CsvTokenTypes>(Identifier), ITokenType<CsvTokenTypes>
{
    public static readonly CsvTokenTypes Comma = new(",");
    public static readonly CsvTokenTypes EndOfRecord = new("\r\n");
    public static readonly CsvTokenTypes DoubleQuote = new("\"");

    public static TokenConfiguration<CsvTokenTypes> Configuration { get; } = new()
    {
        { ",", Comma },
        { "\r\n", EndOfRecord },
        { "\"", DoubleQuote },
    };
    
    public static CsvTokenTypes Create(string token) => new(token);
}
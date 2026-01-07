namespace Tokensharp.Benchmark;

public record CsvTokenTypes(string Lexeme) : TokenType<CsvTokenTypes>(Lexeme), ITokenType<CsvTokenTypes>
{
    public static readonly CsvTokenTypes Comma = new(",");
    public static readonly CsvTokenTypes EndOfRecord = new("\r\n");
    public static readonly CsvTokenTypes DoubleQuote = EndOfRecord.Next("\"");

    public static IEnumerable<CsvTokenTypes> TokenTypes { get; } =
    [
        Comma,
        EndOfRecord,
        DoubleQuote
    ];
    
    public static CsvTokenTypes Create(string token) => new(token);
}
using BenchmarkDotNet.Attributes;

namespace Tokensharp.Benchmark;

[SimpleJob]
[MemoryDiagnoser]
public class CsvBenchmark
{
    [Benchmark]
    public void ParseCsv()
    {
        const string testStr = """
                               test, 123
                               foo,"bar
                               bizz"
                               """;

        ReadOnlySpan<char> csvSpan = testStr.AsSpan();
        while (Tokenizer.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }
}
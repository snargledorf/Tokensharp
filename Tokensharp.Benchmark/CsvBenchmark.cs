using BenchmarkDotNet.Attributes;

namespace Tokensharp.Benchmark;

[SimpleJob]
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

        ReadOnlyMemory<char> csvMemory = testStr.AsMemory();
        while (Tokenizer.TryParseToken(csvMemory, false, out Token<CsvTokenTypes> token))
            csvMemory = csvMemory[token.Lexeme.Length..];
    }
}
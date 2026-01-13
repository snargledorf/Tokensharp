using BenchmarkDotNet.Attributes;
using Tokensharp.StateMachine;

namespace Tokensharp.Benchmark;

[SimpleJob]
[MemoryDiagnoser]
public class CsvBenchmark
{
    private const string TestStr = """
                                   test, 123
                                   foo,"bar
                                   bizz"
                                   """;
    
    private string? _largeTestStr;

    [GlobalSetup]
    public void Setup()
    {
        // Create a larger CSV string for more realistic benchmarking
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            builder.Append(TestStr);
            builder.Append("\r\n");
        }
        _largeTestStr = builder.ToString();
    }
    
    [Benchmark]
    public void ParseCsv_Small()
    {
        ReadOnlySpan<char> csvSpan = TestStr.AsSpan();
        while (Tokenizer.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }

    [Benchmark]
    public void ParseCsv_Large()
    {
        ReadOnlySpan<char> csvSpan = _largeTestStr.AsSpan();
        while (Tokenizer.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }

    [Benchmark]
    public void EnumerateTokens_Small()
    {
        using IEnumerator<Token<CsvTokenTypes>> enumerator = Tokenizer.EnumerateTokens<CsvTokenTypes>(TestStr).GetEnumerator();
        while (enumerator.MoveNext()) ;
    }

    [Benchmark]
    public void EnumerateTokens_Large()
    {
        using IEnumerator<Token<CsvTokenTypes>> enumerator = Tokenizer.EnumerateTokens<CsvTokenTypes>(_largeTestStr!).GetEnumerator();
        while (enumerator.MoveNext()) ;
    }
}
using BenchmarkDotNet.Attributes;
using Tokensharp.StateMachine;

namespace Tokensharp.Benchmark;

[SimpleJob]
[MemoryDiagnoser]
public class CsvBenchmark
{
    private static readonly TokenReaderStateMachine<CsvTokenTypes> TokenReaderStateMachine =
        TokenReaderStateMachine<CsvTokenTypes>.For(CsvTokenTypes.Configuration);
    
    [Benchmark]
    public void ParseCsv()
    {
        const string testStr = """
                               test, 123
                               foo,"bar
                               bizz"
                               """;

        ReadOnlySpan<char> csvSpan = testStr.AsSpan();
        
        while (Tokenizer.TryParseToken(csvSpan, TokenReaderStateMachine, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }
}
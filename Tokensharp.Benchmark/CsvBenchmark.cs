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
    
    private TokenReaderStateMachine<CsvTokenTypes>? _tokenReaderStateMachine;
    
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
        _tokenReaderStateMachine = TokenReaderStateMachine<CsvTokenTypes>.Default;
    }

    [Benchmark]
    public bool TokenParser_SingleToken()
    {
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenReaderStateMachine!);
        return tokenParser.TryParseToken(TestStr, false, out TokenType<CsvTokenTypes>? _, out int _);
    }

    [Benchmark]
    public void TokenParser_Small()
    {
        ReadOnlySpan<char> csvSpan = TestStr.AsSpan();
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenReaderStateMachine!);
        while (tokenParser.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out int length))
            csvSpan = csvSpan[length..];
    }

    [Benchmark]
    public void TokenParser_Large()
    {
        ReadOnlySpan<char> csvSpan = _largeTestStr.AsSpan();
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenReaderStateMachine!);
        while (tokenParser.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out int length))
            csvSpan = csvSpan[length..];
    }
    
    [Benchmark]
    public void ParseCsv_Small()
    {
        ReadOnlySpan<char> csvSpan = TestStr.AsSpan();
        while (Tokenizer.TryParseToken(csvSpan, _tokenReaderStateMachine!, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }

    [Benchmark]
    public void ParseCsv_Large()
    {
        ReadOnlySpan<char> csvSpan = _largeTestStr.AsSpan();
        while (Tokenizer.TryParseToken(csvSpan, _tokenReaderStateMachine!, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }

    [Benchmark]
    public void EnumerateTokens_Small()
    {
        using IEnumerator<Token<CsvTokenTypes>> enumerator = Tokenizer.EnumerateTokens(TestStr, _tokenReaderStateMachine!).GetEnumerator();
        while (enumerator.MoveNext()) ;
    }

    [Benchmark]
    public void EnumerateTokens_Large()
    {
        using IEnumerator<Token<CsvTokenTypes>> enumerator = Tokenizer.EnumerateTokens(_largeTestStr!, _tokenReaderStateMachine!).GetEnumerator();
        while (enumerator.MoveNext()) ;
    }
}
using BenchmarkDotNet.Attributes;

namespace Tokensharp.Benchmark;

[MemoryDiagnoser]
public class CsvBenchmark
{
    private const string TestStr = """
                                   test, 123
                                   foo,"bar
                                   bizz"
                                   """;

    [Params(1, 100, 1000)]
    public int RecordCount { get; set; }

    private string _testData = "";
    private TokenConfiguration<CsvTokenTypes>? _tokenConfiguration;
    
    [GlobalSetup]
    public void Setup()
    {
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < RecordCount; i++)
            builder.AppendLine(TestStr);
        
        _testData = builder.ToString();
        _tokenConfiguration = CsvTokenTypes.Configuration;
    }

    [Benchmark]
    public bool TokenParser_SingleToken()
    {
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenConfiguration!);
        return tokenParser.TryParseToken(_testData, false, out TokenType<CsvTokenTypes>? _, out int _);
    }

    [Benchmark]
    public void TokenParser()
    {
        ReadOnlySpan<char> csvSpan = _testData.AsSpan();
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenConfiguration!);
        while (tokenParser.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out int length))
            csvSpan = csvSpan[length..];
    }

    [Benchmark]
    public void Tokenizer()
    {
        ReadOnlySpan<char> csvSpan = _testData.AsSpan();
        while (Tokensharp.Tokenizer.TryParseToken(csvSpan, _tokenConfiguration!, false, out TokenType<CsvTokenTypes>? _, out ReadOnlySpan<char> lexeme))
            csvSpan = csvSpan[lexeme.Length..];
    }
}
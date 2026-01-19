using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Tokensharp.Benchmark;

[SimpleJob]
[MemoryDiagnoser]
//[Config(typeof(Config))]
public class CsvBenchmark
{
    private const string TestStr = """
                                   test, 123
                                   foo,"bar
                                   bizz"
                                   """;
    
    private string? _largeTestStr;
    private TokenConfiguration<CsvTokenTypes>? _tokenConfiguration;

    public class Config : ManualConfig
    {
        private static readonly string[] TargetVersions = [
            "2.0.0",
            "3.0.0-beta.1",
            "3.0.0-beta.2",
            "3.0.0-beta.3"
        ];
        
        public Config()
        {
            foreach (string version in TargetVersions)
            {
                AddJob(Job.MediumRun
                    //.WithMsBuildArguments($"/p:SwiftStateVersion={version}")
                    //.WithId($"v{version}")
                );
            }

            AddColumnProvider(DefaultColumnProviders.Instance);
            AddLogger(ConsoleLogger.Default);
        }
    }
    
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
        _tokenConfiguration = CsvTokenTypes.Configuration;
    }

    [Benchmark]
    public bool TokenParser_SingleToken()
    {
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenConfiguration!);
        return tokenParser.TryParseToken(TestStr, false, out TokenType<CsvTokenTypes>? _, out int _);
    }

    [Benchmark]
    public void TokenParser_Small()
    {
        ReadOnlySpan<char> csvSpan = TestStr.AsSpan();
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenConfiguration!);
        while (tokenParser.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out int length))
            csvSpan = csvSpan[length..];
    }
    
    [Benchmark]
    public void TokenParser_Large()
    {
        ReadOnlySpan<char> csvSpan = _largeTestStr.AsSpan();
        var tokenParser = new TokenParser<CsvTokenTypes>(_tokenConfiguration!);
        while (tokenParser.TryParseToken(csvSpan, false, out TokenType<CsvTokenTypes>? _, out int length))
            csvSpan = csvSpan[length..];
    }
/*
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
    }*/
}
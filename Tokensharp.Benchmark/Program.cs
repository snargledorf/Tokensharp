using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Tokensharp.Benchmark;

#if DEBUG
var config = new DebugInProcessConfig();
#else
var config = new CsvBenchmark.Config();
#endif

BenchmarkRunner.Run<CsvBenchmark>(config, args);
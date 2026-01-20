using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Tokensharp.Benchmark;

#if DEBUG
var config = new DebugInProcessConfig();
BenchmarkRunner.Run<CsvBenchmark>(config, args);
#else
BenchmarkRunner.Run<CsvBenchmark>();
#endif

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Tokensharp.Benchmark;

#if DEBUG
var config = new DebugInProcessConfig();
#else
BenchmarkRunner.Run<CsvBenchmark>();
#endif
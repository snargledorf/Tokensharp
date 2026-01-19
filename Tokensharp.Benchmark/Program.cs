using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Tokensharp.Benchmark;

#if DEBUG
var config = new DebugInProcessConfig();
BenchmarkRunner.Run<CsvBenchmark>(config, args);
#else
BenchmarkRunner.Run<CsvBenchmark>();
// var csv = new CsvBenchmark();
// csv.Setup();
// for (var i = 0; i < 100_000; i++)
//     csv.TokenParser_Large();
#endif

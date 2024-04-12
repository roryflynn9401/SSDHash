using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HashAnalyser.Data.Models;
using Newtonsoft.Json;
using SSDHash;

namespace LogAnalyser.Testing
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net70)]
    public class SSDHashPerformanceBenchmarks
    {
        private const string S1Path = "F:\\\\source\\repos\\ResearchProject\\LogAnalyser\\bin\\Debug\\net7.0\\trainingData.csv";
        private const string S4Path = "F:\\\\source\\repos\\ResearchProject\\LogAnalyser\\bin\\Debug\\net7.0\\apidata.csv";

        private string[]? S1Objects;
        private string[]? S4Objects;

        [GlobalSetup]
        public async Task Setup()
        {
            S1Objects = await HashProcessing.GetFileObjects(S1Path);
            S4Objects = await HashProcessing.GetFileObjects(S4Path);
            if (S1Objects is null) throw new InvalidDataException(nameof(S1Objects));
            if (S4Objects is null) throw new InvalidDataException(nameof(S4Objects));
        }

        #region S1 Tests

        [Benchmark]
        public async Task IndividualSSDHashS1() => await HashProcessing.ComputeHashesFromFile(S1Path);

        [Benchmark]
        public async Task ParallelSSDHashS1_10() => await S1SSDHashPerfBenchmark(10);

        [Benchmark]
        public async Task ParallelSSDHashS1_100() => await S1SSDHashPerfBenchmark(100);

        [Benchmark]
        public async Task ParallelSSDHashS1_1000() => await S1SSDHashPerfBenchmark(1000);

        [Benchmark]
        public async Task ParallelSSDHashS1_10000() => await S1SSDHashPerfBenchmark(10000);

        #endregion

        #region S4 Tests

        [Benchmark]
		public async Task IndividualSSDHashS4() => await HashProcessing.ComputeHashesFromFile(S4Path);

        [Benchmark]
		public async Task ParallelSSDHashS4_10() => await S4SSDHashPerfBenchmark(10);

        [Benchmark]
        public async Task ParallelSSDHashS4_100() => await S4SSDHashPerfBenchmark(100);

        [Benchmark]
        public async Task ParallelSSDHashS4_1000() => await S4SSDHashPerfBenchmark(1000);

        [Benchmark]
        public async Task ParallelSSDHashS4_10000() => await S4SSDHashPerfBenchmark(10000);

        #endregion

        #region Helpers 

        private async Task S1SSDHashPerfBenchmark(int count)
        {
            var data = S1Objects.Take(count).ToArray();
            await HashProcessing.ComputeHashes(data);
        }
        private async Task S4SSDHashPerfBenchmark(int count)
        {
            var data = S4Objects.Take(count).ToArray();
            await HashProcessing.ComputeHashes(data);
        }

        #endregion
    }
}
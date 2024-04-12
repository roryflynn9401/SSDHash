using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HashAnalyser.Clustering;
using HashAnalyser.Data;
using HashAnalyser.Data.Models;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Prediction;
using Microsoft.ML;
using Newtonsoft.Json;
using SSDHash;
using System.Diagnostics;

namespace LogAnalyser.Testing
{
    [SimpleJob(runtimeMoniker: RuntimeMoniker.Net70)]
    public class PipelinePerformanceBenchmarks
    {
        private HashClassificationPredictor _classificationPredictor = new();
		private string[] hashes;

        // Unfortunately due to the nature of BenchMarkDotnet compiling and running each test seperately, there is no way to pass paramaeters to the tests. This means file inputs have to be hardcoded.
        [GlobalSetup]
        public void Setup()
        {
            var formatter = new TrainingDataFormatter("""F:\\source\repos\ResearchProject\LogAnalyser\bin\Debug\net7.0\trainingData.csv""");
            var hs = formatter.LoadFile("""F:\\source\repos\ResearchProject\LogAnalyser\bin\Debug\net7.0\trainingData.csv""", 100000);
            hashes = hs.ToArray();

        }

        [Benchmark]
        public void PipelinePrediction_1000() => Pipeline(1000);

        [Benchmark]
        public void PipelinePrediction_10000() => Pipeline(10000);

        [Benchmark]
        public void PipelinePrediction_100000() => Pipeline(100000);


        private void Pipeline(int count)
        {
            var input = hashes.Take(count).ToArray();
            var binaryResults = _classificationPredictor.PredictBinary(input, """F:/source/repos/ResearchProject/LogAnalyser/Models/BinaryModel.zip""");
            if (binaryResults is null) return;

            var context = new MLContext(0);
            var binResults = context.Data.CreateEnumerable<BinaryHashPrediction>(binaryResults, false).ToArray();

            var malPredictions = binResults.Where(x => x.PredictedLabel).Select(x => new MulticlassHashModel(x.Hash));
            var multDataview = context.Data.LoadFromEnumerable(malPredictions);

            var multiclassResults = _classificationPredictor.PredictMulticlass(multDataview, """F:/source/repos/ResearchProject/LogAnalyser/Models/MulticlassModel.zip""");
            if (multiclassResults is null) return;

            var multPredictions = context.Data.CreateEnumerable<MulticlassHashPrediction>(multiclassResults, false).ToArray();
        }
    }
}
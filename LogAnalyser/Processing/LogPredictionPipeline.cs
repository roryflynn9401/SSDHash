using CsvHelper;
using HashAnalyser.Clustering;
using HashAnalyser.Data;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Prediction;
using Microsoft.ML;
using Newtonsoft.Json;
using SSDHash;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LogAnalyser.Processing
{
    public class LogPredictionPipeline
    {
        private HashClassificationPredictor _classificationPredictor = new();
        private HashExtractor _hashExtractor = new();

        public LogPredictionPipeline() { }

        public MulticlassHashPrediction[]? ProcessLogs(string fileInput)
        {
            var formatter = new TrainingDataFormatter(fileInput);
            var hashes = formatter.LoadFile(fileInput).ToArray();

            var binaryResults = _classificationPredictor.PredictBinary(hashes);
            if (binaryResults is null) return null;

            var context = new MLContext(0);
            var binResults = context.Data.CreateEnumerable<BinaryHashPrediction>(binaryResults, false).ToArray();

            Console.WriteLine($"""

                Binary: 

                Benign Predictions: {binResults.Count(x => !x.PredictedLabel)}
                Malicious Predictions: {binResults.Count(x => x.PredictedLabel)}
                """);

            var malPredictions = binResults.Where(x => x.PredictedLabel).Select(x => new MulticlassHashModel(x.Hash));
            var multDataview = context.Data.LoadFromEnumerable(malPredictions);

            var multiclassResults = _classificationPredictor.PredictMulticlass(multDataview);
            if (multiclassResults is null) return null;

            var multPredictions = context.Data.CreateEnumerable<MulticlassHashPrediction>(multiclassResults, false).ToArray();

            Console.WriteLine($"""
                
                Mutliclass:

                Port-Scan Predictions: {multPredictions.Count(x => x.PredictedLabel == "port-scan")}
                C&C Predictions: {multPredictions.Count(x => x.PredictedLabel == "c&c")}
                DOS Predictions: {multPredictions.Count(x => x.PredictedLabel == "dos")}
                """);
            return multPredictions;
        }
    }
}

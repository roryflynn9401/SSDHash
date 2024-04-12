using HashAnalyser.Clustering;
using HashAnalyser.Data;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Clustering;
using HashAnalyser.Data.Models.Multiclass;
using HashAnalyser.Training.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Tokenizers;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;
using System.Data;
using TorchSharp;
using static Microsoft.ML.DataOperationsCatalog;

namespace HashAnalyser.Training
{
    public class ClusteringHashAnalysisTrainer : HashAnalysisTrainer
    {
        public ClusteringHashAnalysisTrainer(DeviceType deviceType = DeviceType.CUDA) : base(deviceType) { }

        public override void TrainModel(string dataSetFileName)
        {
            var dataFormatter = new TrainingDataFormatter(dataSetFileName);
            var data = dataFormatter.LoadFileForClustering(dataSetFileName);

            var mlContext = new MLContext(0);
            var model = KMeansClusteringTransformer.GetModel(mlContext);

            var dataView = mlContext.Data.LoadFromEnumerable(data);
            //var trainedModel = Train(model, dataView, "ClusterModel.zip");
            //Evaluate(trainedModel, dataView);
            Visualize(dataView);
        }

        protected override void Evaluate(ITransformer model, IDataView data)
        {
            
            Console.WriteLine("=====================Evaluating model performance on test set=====================\n");
            IDataView transformedTest = model.Transform(data);

            ClusteringMetrics metrics = _mlContext.Clustering.Evaluate(transformedTest, labelColumnName: "Label", featureColumnName: "NgramFeatures");

            Console.WriteLine($""" 
            Average Distance : {metrics.AverageDistance}
            """);

        }


        public void Visualize(IDataView data, string modelName = "ClusterModel.zip")
        {
            VBuffer<float>[] centroids = default;
            var mlContext = new MLContext(0);

            var model = KMeansClusteringTransformer.GetModel(mlContext);

            data = _mlContext.Data.ShuffleRows(data);
            TrainTestData trainValidationData = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            Console.WriteLine("==================Begining training==================\n");
            var trainedModel = model.Fit(trainValidationData.TrainSet);
            Console.WriteLine("==================Training complete==================\n");

            Console.WriteLine("Saving model... \n");
            _mlContext.Model.Save(trainedModel, data.Schema, modelName);
            Console.WriteLine("=====================Model saved=====================\n");


            var last = trainedModel.LastTransformer.LastTransformer.Model;
            last.GetClusterCentroids(ref centroids, out var k);

            var cleanCentroids = Enumerable.Range(1, 4).ToDictionary(x => (uint)x, x =>
            {
                var values = centroids[x - 1].GetValues().ToArray();
                return values;
            });

            var getNgrams = trainedModel.Transform(data);

            var evaulation = mlContext.Clustering.Evaluate(getNgrams, labelColumnName: "Label");

            Console.WriteLine($"""
                {evaulation.DaviesBouldinIndex}
                {evaulation.NormalizedMutualInformation}
                {evaulation.AverageDistance}
                """);
            var ngrams = mlContext.Data.CreateEnumerable<ClusteringTransformedHash>(getNgrams, false).ToArray();

            var points = new Dictionary<uint, List<(double X, double Y)>>();

            var predictionEngine = mlContext.Model.CreatePredictionEngine<ClusteringHashModel, ClusteringHashPrediction>(trainedModel,data.Schema);
            foreach (var dp in ngrams)
            {
                var prediction = predictionEngine.Predict(dp);

                var weightedCentroid = cleanCentroids[prediction.PredictedLabel].Zip(dp.NgramFeatures, (x, y) => x * y);
                var point = (X: weightedCentroid.Take(weightedCentroid.Count() / 2).Sum(), Y: weightedCentroid.Skip(weightedCentroid.Count() / 2).Sum());

                if (!points.ContainsKey(prediction.PredictedLabel))
                    points[prediction.PredictedLabel] = new List<(double X, double Y)>();
                points[prediction.PredictedLabel].Add(point);

            }

            var imageGen = new ImageGenerator();
            imageGen.ShowClusters(points);
        }
    }
}

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Text;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashAnalyser.Training.Models
{
    internal static class KMeansClusteringTransformer
    {
        internal static EstimatorChain<TransformerChain<ClusteringPredictionTransformer<KMeansModelParameters>>> GetModel(MLContext mlContext) => mlContext.Transforms.Conversion.MapValueToKey("Label")
                                                                                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Hash"))
                                                                                    .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
                                                                                    .Append(mlContext.Transforms.Text.ProduceNgrams("NgramFeatures",
                                                                                        inputColumnName: "Tokens",
                                                                                        ngramLength: 3,
                                                                                        useAllLengths: false,
                                                                                        weighting: NgramExtractingEstimator.WeightingCriteria.Idf)
                                                                                    .Append(mlContext.Transforms.NormalizeLogMeanVariance("NgramFeatures", "NgramFeatures"))
                                                                                    .Append(mlContext.Clustering.Trainers.KMeans("NgramFeatures", numberOfClusters: 4)));
    }
}

using Microsoft.ML;
using Microsoft.ML.Transforms.Text;

namespace HashAnalyser.Training.Models
{
    internal static class BinaryHashTransformer
    {
        internal static IEstimator<ITransformer> GetModel(MLContext mlContext) => mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Hash")
                                .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
                                .Append(mlContext.Transforms.Text.ProduceNgrams("NgramFeatures",
                                    inputColumnName: "Tokens",
                                    ngramLength: 3,
                                    useAllLengths: false,
                                    weighting: NgramExtractingEstimator.WeightingCriteria.Idf)
                                .Append(mlContext.BinaryClassification.Trainers.LinearSvm(labelColumnName: "Label", featureColumnName: "NgramFeatures")));
    }
}

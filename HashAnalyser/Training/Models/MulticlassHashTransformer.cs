using Microsoft.ML;
using Microsoft.ML.Transforms.Text;

namespace HashAnalyser.Training.Models
{
    internal class MulticlassHashTransformer
    {
        //PlaceHolder
        internal static IEstimator<ITransformer> GetModel(MLContext mlContext) => mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Hash")
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
                .Append(mlContext.Transforms.Text.ProduceNgrams("NgramFeatures",
                    inputColumnName: "Tokens",
                    ngramLength: 3,
                    useAllLengths: false,
                    weighting: NgramExtractingEstimator.WeightingCriteria.Idf)
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "NgramFeatures")));
    }
}

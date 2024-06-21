using HashAnalyser.Data.Models;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Image;
using Microsoft.ML.Transforms.Text;
using SkiaSharp;
using System.Globalization;

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
                                    weighting: NgramExtractingEstimator.WeightingCriteria.Idf))
                                .Append(mlContext.BinaryClassification.Trainers.LdSvm(labelColumnName: "Label", featureColumnName: "NgramFeatures"));

        internal static IEstimator<ITransformer> GetOneHotModel(MLContext mlContext) => mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Hash")
                                .Append(mlContext.Transforms.Categorical.OneHotEncoding("PositionTokens", "Hash"))
                                .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
                                .Append(mlContext.Transforms.Text.ProduceNgrams("NgramFeatures",
                                    inputColumnName: "Tokens",
                                    ngramLength: 3,
                                    useAllLengths: false,
                                    weighting: NgramExtractingEstimator.WeightingCriteria.Idf))
                                .Append(mlContext.Transforms.Concatenate("Features", "NgramFeatures", "PositionTokens"))
                                .Append(mlContext.BinaryClassification.Trainers.LdSvm(labelColumnName: "Label", featureColumnName: "Features"));

        internal static IEstimator<ITransformer> GetRawLogModel(MLContext mlContext) => mlContext.Transforms.Concatenate("LogText", nameof(RawLogRecord.ts), nameof(RawLogRecord.uid), nameof(RawLogRecord.id_orig_h),
                nameof(RawLogRecord.id_orig_p), nameof(RawLogRecord.id_resp_h), nameof(RawLogRecord.id_resp_p), nameof(RawLogRecord.proto), nameof(RawLogRecord.service),
                nameof(RawLogRecord.duration), nameof(RawLogRecord.orig_bytes), nameof(RawLogRecord.resp_bytes), nameof(RawLogRecord.conn_state), nameof(RawLogRecord.local_orig),
                nameof(RawLogRecord.local_resp), nameof(RawLogRecord.missed_bytes), nameof(RawLogRecord.history), nameof(RawLogRecord.orig_pkts), nameof(RawLogRecord.orig_ip_bytes),
                nameof(RawLogRecord.resp_pkts), nameof(RawLogRecord.resp_ip_bytes))
                                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokenizedText", "LogText"))
                                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("Embeddings", "TokenizedText", WordEmbeddingEstimator.PretrainedModelKind.GloVe300D))
                                .Append(mlContext.BinaryClassification.Trainers.LdSvm(labelColumnName: "label", featureColumnName: "Embeddings"));


        internal static IEstimator<ITransformer> GetImageModel(MLContext mlContext, string dataSetFileName) => mlContext.Transforms.Conversion.MapValueToKey("Label")
                                .Append(mlContext.MulticlassClassification.Trainers.ImageClassification(featureColumnName: "input"))
                                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

    }

}

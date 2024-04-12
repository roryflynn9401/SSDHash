using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Clustering
{
    public class ClusteringHashPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedLabel { get; set; }

        [ColumnName("Score")]
        public float[] Score { get; set; }
    }
}

using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Binary
{
    public class BinaryHashPrediction
    {
        public string Hash { get; set; }

        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }
    }
}

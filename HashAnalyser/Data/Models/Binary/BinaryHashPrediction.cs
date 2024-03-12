using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Binary
{
    public class BinaryHashPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }
    }
}

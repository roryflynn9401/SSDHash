using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Multiclass
{
    public class MulticlassHashPrediction
    {
        public string Hash { get; set; }
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; }
    }
}

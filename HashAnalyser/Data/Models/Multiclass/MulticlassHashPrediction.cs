using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Multiclass
{
    public class MulticlassHashPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }
    }
}

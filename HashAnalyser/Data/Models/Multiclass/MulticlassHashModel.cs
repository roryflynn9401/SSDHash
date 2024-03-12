using Microsoft.ML.Data;
using System;

namespace HashAnalyser.Data.Models.Multiclass
{
    public class MulticlassHashModel
    {
        public MulticlassHashModel() { }
        public MulticlassHashModel(string hash, string? label = null)
        {
            Hash = hash;
            if (label is not null) Label = label;
        }


        public string Hash { get; set; }

        [ColumnName(@"Label")]
        public string? Label { get; set; }
    }

    public class MulticlassTransformedHash : MulticlassHashModel
    {
        public float[] NgramFeatures { get; set; }
    }
}

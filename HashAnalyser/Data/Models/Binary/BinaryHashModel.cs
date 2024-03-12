using Microsoft.ML.Data;
using System;

namespace HashAnalyser.Data.Models.Binary
{
    public class BinaryHashModel
    {
        public BinaryHashModel() { }
        public BinaryHashModel(string hash, bool? label = null)
        {
            Hash = hash;
            if (label is not null) Label = label.Value;
        }


        public string Hash { get; set; }

        [ColumnName(@"Label")]
        public bool Label { get; set; }
    }

    public class BinaryTransformedHash : BinaryHashModel
    {
        public float[] NgramFeatures { get; set; }
    }
}

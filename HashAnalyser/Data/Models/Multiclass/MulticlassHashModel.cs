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

    public class MulticlassInput : MulticlassHashModel
    {

        public MulticlassInput() : base() { }
        public MulticlassInput(string hash, string? label = null) : base(hash, label) { }

        [VectorType]
        public float[] NgramFeatures { get; set; }
        [VectorType]
        public float[] Tokens { get; set; }
        [VectorType]
        public float[] PositionTokens { get; set; }
    }

    public class MulticlassOutput : MulticlassHashModel
    {

        public MulticlassOutput() : base() { }
        public MulticlassOutput(string hash, string? label = null) : base(hash, label) { }

        [VectorType]
        public float[] NgramFeatures { get; set; }
        [VectorType]
        public float[] Tokens { get; set; }
        [VectorType]
        public float[] PositionTokens { get; set; }
        [VectorType]
        public float[] Features { get; set; }
    }
}

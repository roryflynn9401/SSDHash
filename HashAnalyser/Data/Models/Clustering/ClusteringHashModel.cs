using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models.Clustering
{
    public class ClusteringHashModel
    {
        public ClusteringHashModel() { }
        public ClusteringHashModel(string hash, uint? label = null)
        {
            Hash = hash;
            if (label is not null) Label = label.Value;
        }


        public string Hash { get; set; }

        [ColumnName(@"Label")]
        public uint Label { get; set; }
    }

    public class ClusteringTransformedHash : ClusteringHashModel
    {
        public float[] NgramFeatures { get; set; }
    }
}

using Microsoft.ML.Data;

namespace HashAnalyser.Data.Models
{
    public class HashModel
    {

        public HashModel(string hash)
        {
            Hash = hash;
        }

        public HashModel(string hash, string label)
        {
            Hash = hash;
            Label = label;
        }

        public string Hash { get; set; }

        [ColumnName(@"Label")]
        public string? Label { get; set; }
    }
}

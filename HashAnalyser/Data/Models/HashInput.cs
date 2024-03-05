namespace HashAnalyser.Data.Models
{
    public class HashInput
    {

        public HashInput(string hash)
        {
            Hash = hash;
        }

        public string? Hash { get; set; }
    }
}

using CsvHelper;
using HashAnalyser.Data;
using System.Globalization;

namespace HashAnalyser.Configuration
{
    /// <summary> 
    /// Class containing configuration data
    /// </summary>
    public static class Config
    {
        public static string EncodingFilePath = $"{Environment.CurrentDirectory}/../../../Configuration/Files/PositionalEmbeddings.csv";

        private static Dictionary<int, string>? positionalEncoding;

        public static Dictionary<int, string> GetPositionalEncoding()
        {
            if (positionalEncoding is null)
            {
                positionalEncoding = new Dictionary<int, string>();
                using (var sr = new StreamReader(EncodingFilePath))
                {
                    using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<PositionalEncodingModel>().ToArray();
                        foreach (var record in records)
                        {
                            positionalEncoding[record.InputId] = record.EncodedOutput.ToLower();
                        }
                    }
                }

            }
            return positionalEncoding;
        }
    }
}

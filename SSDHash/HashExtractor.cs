using Newtonsoft.Json;
using SpookilySharp;
using SSDHash.Preprocessing;
using System.Text;
using System.Xml.Linq;

namespace SSDHash
{
    public class HashExtractor
    {
        public string? GetHash(string input, IDataProcessor? processor = null)
        {
            var dataProcessor = processor == null ? GetDataProcessor(input) : processor;

            //Ensure valid input
            if (dataProcessor is null) return null;
            var inputDict = dataProcessor.Convert(input);
            if (inputDict is null) return null;

            Dictionary<string, string[]> workingInput = TokenizeData(inputDict);
            Dictionary<string, string[]> prependedInput = PrependFieldNames(workingInput);
            Dictionary<string, int[]> hashBuckets = HashFieldValues(prependedInput);
            Dictionary<int, int> bucketCounts = CountBuckets(hashBuckets.Values.SelectMany(x => x).ToArray());
            Dictionary<int, int> scaledCounts = ScaleAndQuantize(bucketCounts, 15);
            string hash = GenerateHashDigest(scaledCounts);

            return hash;
        }


        private Dictionary<string, string[]> TokenizeData(Dictionary<string, string> input)
        {
            var dict = new Dictionary<string, string[]>();

            foreach (var item in input)
            {
                var tokenizedValues = item.Value.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries);
                dict.Add(item.Key, tokenizedValues);
            }

            return dict;
        }

        private Dictionary<string, string[]> PrependFieldNames(Dictionary<string, string[]> fields)
        {
            var tokenizedValues = new Dictionary<string, string[]>();

            foreach (var entry in fields)
            {
                var prependedValues = new string[entry.Value.Length];
                for (int i = 0; i < entry.Value.Length; i++)
                {
                    prependedValues[i] = $"{entry.Key}:{entry.Value[i]}";
                }

                tokenizedValues.Add(entry.Key, prependedValues);
            }

            return tokenizedValues;
        }

        private Dictionary<string, int[]> HashFieldValues(Dictionary<string, string[]> values)
        {
            var hashBuckets = new Dictionary<string, int[]>();

            foreach (var entry in values)
            {
                var bucketValues = new int[entry.Value.Length];
                int i = 0;
                foreach (var value in entry.Value)
                {
                    var bytes = Encoding.UTF8.GetBytes(value);
                    ulong hashValue = Hash64(bytes, bytes.Length, 0);

                    var bucket = hashValue % 64;
                    bucketValues[i] = (int)bucket;
                    i++;
                }
                hashBuckets.Add(entry.Key, bucketValues);
            }

            return hashBuckets;
        }

        private unsafe ulong Hash64(byte[] data, int length, ulong seed)
        {
            fixed (byte* pMessage = data)
            {
                return SpookyHash.Hash64(pMessage, length, seed);
            }
        }

        private Dictionary<int, int> CountBuckets(int[] buckets)
        {
            var bucketCounts = new Dictionary<int, int>();

            foreach (var bucket in buckets)
            {
                if (bucketCounts.ContainsKey(bucket))
                {
                    bucketCounts[bucket]++;
                }
                else
                {
                    bucketCounts[bucket] = 1;
                }
            }

            return bucketCounts;
        }

        private Dictionary<int, int> ScaleAndQuantize(Dictionary<int, int> bucketCounts, int levels)
        {
            int totalOccurrences = bucketCounts.Values.Sum();

            Dictionary<int, double> scaledCounts = bucketCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => (double)kvp.Value / totalOccurrences * levels
            );

            var sortedCounts = scaledCounts.OrderByDescending(x => x.Value).ToList();
            var quantizedCounts = new Dictionary<int, int>();

            for (int i = 0; i < sortedCounts.Count; i++)
            {
                int bucket = sortedCounts[i].Key;
                int rank = (int)Math.Round((double)i / (sortedCounts.Count - 1) * (levels - 1));
                quantizedCounts[bucket] = rank;
            }

            return quantizedCounts;
        }

        private string GenerateHashDigest(Dictionary<int, int> quantizedCounts)
        {
            StringBuilder hashDigest = new StringBuilder();

            for (int i = 0; i < 64; i++)
            {
                if (quantizedCounts.ContainsKey(i))
                {
                    hashDigest.Append(quantizedCounts[i].ToString("X")[0]);
                }
                else
                {
                    hashDigest.Append("0");
                }
            }

            return hashDigest.ToString();
        }

        #region Helper Methods 

        private bool IsValidJson(string inputString)
        {
            try
            {
                JsonConvert.DeserializeObject(inputString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private bool IsValidXml(string inputString)
        {
            try
            {
                XDocument.Parse(inputString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public virtual IDataProcessor? GetDataProcessor(string input)
        {
            if (IsValidJson(input))
            {
                return new JsonProcessor();
            }
            else if (IsValidXml(input))
            {
                return new XmlProcessor();
            }

            return null;
        }

        #endregion
    }
}

using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SSDHash
{
    public static class HashProcessing
    {
        public static double CalculateDissimilarity(string hash1, string hash2)
        {
            var xlen = hash2.Length;
            var z = Enumerable.Range(0, xlen);
            var x = hash2.Select(c => MapHex(c));
            var y = hash1.Select(c => MapHex(c));
            if(x.Any(x => x is null) || y.Any(x => x is null)) { return -1; }

            var result = z
                .Select(zVal =>
                {
                    var xi = x.ElementAt(zVal);
                    var yi = y.ElementAt(zVal);

                    var dmax = xi > yi ? xi : yi;
                    var dmin = xi < yi ? xi : yi;

                    return new { dmax, dmin };
                });

            var sumDmax = result.Sum(r => r.dmax.Value);
            var sumDmin = result.Sum(r => r.dmin.Value);

            var dissim = 1 - ((double)sumDmin / sumDmax);

            return dissim;

        }

        private static uint? MapHex(char c)
        {
            return (uint?)c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'A' => 10,
                'B' => 11,
                'C' => 12,
                'D' => 13,
                'E' => 14,
                'F' => 15,
                _ => null,
            };
        }

        public static async Task<string[]?> ComputeHashesFromFile(string fileInput)
        {
            var inputObjects = await GetFileObjects(fileInput);
            if (inputObjects is null) return null;

            var hashes = await ComputeHashes(inputObjects);
            return hashes;
        }

        public static async Task<string[]> ComputeHashes(string[] inputs)
        {
            var tasks = new List<Task<string>>();

            foreach (var input in inputs)
            {
                tasks.Add(ComputeHash(input));
            }


            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();

        }

        public static async Task<string[]?> GetFileObjects(string fileInput)
        {
            var lowerInput = fileInput.ToLower();
            object[]? inputObjects;

            if (lowerInput.Contains(".json"))
            {
                var input = File.ReadAllText(fileInput);
                inputObjects = ParseJsonData(input);
            }
            else if (lowerInput.Contains(".xml"))
            {
                var input = File.ReadAllText(fileInput);
                inputObjects = ParseXMLData(input);
            }
            else if (lowerInput.Contains(".csv"))
            {
                inputObjects = ParseCsvData(fileInput);
            }
            else
            {
                Console.WriteLine("Unsupported file type for input.");
                return null;
            }

            var hashInputs = inputObjects.Select(x => JsonConvert.SerializeObject(x)).ToArray();
            return hashInputs;
        }

        private static async Task<string> ComputeHash(string input)
        {
            var hashExtractor = new HashExtractor();
            return hashExtractor.GetHash(input) ?? string.Empty;
        }

        private static object[]? ParseJsonData(string data)
        {
            try
            {
                object[]? splitData = JsonConvert.DeserializeObject<object[]>(data);
                if (splitData is null)
                {
                    Console.WriteLine("Invalid JSON data detected.  Input should be an array ");
                    return null;
                }

                return splitData;

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Invalid JSON data detected.  " + e.Message);
            }

            return null;
        }

        private static object[]? ParseXMLData(string data)
        {
           var objects = new List<object>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(data);
            var serializer = new XmlSerializer(typeof(object));

            if (xmlDoc?.DocumentElement?.ChildNodes.Count != 0)
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    using (var reader = new StringReader(node.OuterXml))
                    {
                        var obj = serializer.Deserialize(reader);
                        objects.Add(obj);
                    }
                }
            }
            else
            {
                using (var reader = new StringReader(data))
                {
                    var obj = serializer.Deserialize(reader);
                    objects.Add(obj);
                }
            }

            return objects.ToArray();
        }

        private static object[]? ParseCsvData(string fileName)
        {
            IEnumerable<object> records;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    using (var cr = new CsvReader(sr, CultureInfo.InvariantCulture))
                    {
                        records = cr.GetRecords<object>();
                        return records.ToArray();
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("Invalid Csv data detected.  " + e.Message);
            }

            return null;
        }

    }
}
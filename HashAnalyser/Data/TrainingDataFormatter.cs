using CsvHelper;
using CsvHelper.Configuration;
using HashAnalyser.Configuration;
using HashAnalyser.Data.Models;
using HashAnalyser.Data.Models.Binary;
using HashAnalyser.Data.Models.Multiclass;
using Newtonsoft.Json;
using SSDHash;
using System.Globalization;
using System.Text;

namespace HashAnalyser.Data
{
    public class TrainingDataFormatter
    {
        #region Properties

        private string[] _filePaths;
        private int _maxRowsPerChunk = 100000;
        private int _rowCount = 100000;
        private HashExtractor _hashExtractor = new();
        private string _outputFilePath = "out.csv";

        #endregion

        #region Constructors

        /// <summary> 
        /// Class for formatting training data. Provides utilities to parse data using batching to create a suitable training set. 
        /// </summary>
        /// <remarks>
        /// Supply file inputs using <paramref name="inpuFilePath"/>, which parses <paramref name="maxRowsPerChunk"/> rows per batch and appends the values to the output file suppled using <paramref name="outputFilePath"/>
        /// </remarks>
        public TrainingDataFormatter(string inpuFilePath, int maxRowsPerChunk = 100000, string outputFilePath = "out.csv")
        {
            _filePaths = new[] { inpuFilePath };
            _maxRowsPerChunk = maxRowsPerChunk;
            _outputFilePath = outputFilePath;

        }
        /// <summary> 
        /// Class for formatting training data. Provides utilities to parse data using batching to create a suitable training set. 
        /// </summary>
        /// <remarks>
        /// Supply file inputs using <paramref name="inpuFilePath"/>, which parses <paramref name="maxRowsPerChunk"/> rows per batch and appends the values to the output file suppled using <paramref name="outputFilePath"/>
        /// </remarks>
        public TrainingDataFormatter(string[] inpuFilePaths, int maxRowsPerChunk = 100000, string outputFilePath = "out.csv")
        {
            _filePaths = inpuFilePaths;
           _maxRowsPerChunk = maxRowsPerChunk;
           _outputFilePath = outputFilePath;
        }

        #endregion

        #region Public Methods

        /// <summary> 
        /// Method when called will parse and compute hashes for the data supplied from <paramref name="_filePaths"></paramref> and write the output to <paramref name="_outputFilePath"></paramref>
        /// </summary>
        public async Task ParseDatasetFiles()
        {
            for (int i = 0; i <= _filePaths.Length; i++)
            {
                var records = await ParseFile(i);
                var hashes = await ComputeHashes(records);
                await WriteFile((i != 0), hashes);

                Console.WriteLine($"File {i} succesfully appended to main training file");
            }
        }

        /// <summary> 
        /// Method to load formatted training data from the file specifed in <paramref name="filePath"/>. If no <paramref name="maxCount"/> is suppled, the whole file will be read.
        /// </summary>
        public IEnumerable<BinaryHashModel> LoadFileForBinary(string filePath, int? maxCount = null)
        {
            var bCount = 0;

            using (var reader = new StreamReader(filePath))
            {
                using (var cr = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", Encoding = Encoding.UTF8 }))
                {
                    var count = 0;

                    while (cr.Read())
                    {
                        var record = cr.GetRecord<Log>();
                        if (record.Label.ToLower() == "port-scan") { continue; }
                        if (maxCount != null && count > maxCount) { yield break; }
                        if (record.Hash.Length != 64) { continue; }
                        if (record.Label.ToLower() == "benign") { bCount++; }
                        if (record.Label.ToLower() == "benign" && bCount > 25000) { continue; }
                        count++;

                        yield return new BinaryHashModel(PositionallyEncode(record.Hash), MapBinaryLabel(record.Label.ToLower()));
                    }
                }

            }
        }

        /// <summary> 
        /// Method to load formatted training data from the file specifed in <paramref name="filePath"/>. If no <paramref name="maxCount"/> is suppled, the whole file will be read.
        /// </summary>
        public IEnumerable<MulticlassHashModel> LoadFileForMulticlass(string filePath, int? maxCount = null)
        {
            using (var reader = new StreamReader(filePath))
            {
                using (var cr = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", Encoding = Encoding.UTF8 }))
                {
                    var count = 0;

                    while (cr.Read())
                    {
                        var record = cr.GetRecord<Log>();

                        if (maxCount != null && count > maxCount) { yield break; }
                        if (record.Hash.Length != 64) { continue; }
                        count++;

                        yield return new MulticlassHashModel(PositionallyEncode(record.Hash), record.Label.ToLower());
                    }
                }

            }
        }

        #endregion

        #region Private Methods 

        /// <summary> 
        /// Method to positionally encode the hash suppled in <paramref name="input"/>. Encodes individual hash characters as a word, retaining order. Returns encoded hash as string.
        /// </summary>
        public string PositionallyEncode(string input)
        {
            var map = Config.GetPositionalEncoding();
            var sb = new StringBuilder();

            var values = input.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            var hashPrespaced = values.Length == 64;

            for (int i = 0; i < (hashPrespaced ? values.Length : input.Length); i++)
            {
                var posZero = i * 16;
                var hexInput = $"{input[i]}";
                var hexValue = int.Parse(hashPrespaced ? values[i] : hexInput , NumberStyles.AllowHexSpecifier);
                var encodedIndex = posZero + (hexValue);
                sb.Append(map[encodedIndex] + " ");
            }

            return sb.ToString();
        }

        /// <summary> 
        /// Method to return encoded hash, back to its hexidecimal representation.
        /// </summary>
        public string PositionallyDecode(string encodedString)
        {
            var map = Config.GetPositionalEncoding();
            var sb = new StringBuilder();
            var input = encodedString.Split(" ");

            for (int i = 0; i < input.Length; i++)
            {
                if (string.IsNullOrEmpty(input[i])) continue;

                var encodedIndex = map.First(x => x.Value == input[i]).Key;
                var hexValue = encodedIndex - (i * 16);
                sb.Append(hexValue.ToString("X") + " ");
            }
            return sb.ToString();
        }

        /// <summary> 
        /// Method that parses and cleans the raw dataset files
        /// </summary>
        protected async Task<List<(RawLogRecord, RawLabelRecord)>> ParseFile(int fileIndex)
        {
            var records = new List<(RawLogRecord, RawLabelRecord)>();

            try
            {
                using (var sr = new StreamReader(_filePaths[fileIndex]))
                {

                    var dataStarted = false;
                    while (!sr.EndOfStream)
                    {
                        var row = sr.ReadLine();
                        _rowCount++;
                        if (_rowCount > _maxRowsPerChunk)
                        {
                            Console.WriteLine($"Batching file as length is over {_maxRowsPerChunk}");

                            var maxBatchHashes = await ComputeHashes(records);
                            await WriteFile((fileIndex != 0), maxBatchHashes);
                            records.Clear();
                            _rowCount = 0;
                        }
                        if (row?.Contains("#types") == true)
                        {
                            dataStarted = true;
                            continue;
                        }
                        if (!dataStarted || row is null)
                        {
                            continue;
                        }

                        var data = row.Split("\x09");


                        string[] classDataArr = data.Last().Split(" ");
                        classDataArr = classDataArr.Where(x => !string.IsNullOrEmpty(x) && !x.Contains("-") && !x.Contains("empty")).ToArray();
                        var labels = new[] { "malicious", "benign" };

                        var label = classDataArr.FirstOrDefault(x => labels.Contains(x.ToLower()));
                        if (data.Length <= 20) continue;

                        records.Add((new RawLogRecord
                        {
                            ts = data[0],
                            uid = data[1],
                            id_orig_h = data[2],
                            id_orig_p = data[3],
                            id_resp_h = data[4],
                            id_resp_p = data[5],
                            proto = data[6],
                            service = data[7],
                            duration = data[8],
                            orig_bytes = data[9],
                            resp_bytes = data[10],
                            conn_state = data[11],
                            local_orig = data[12],
                            local_resp = data[13],
                            missed_bytes = data[14],
                            history = data[15],
                            orig_pkts = data[16],
                            orig_ip_bytes = data[17],
                            resp_pkts = data[18],
                            resp_ip_bytes = data[19],
                        }, new RawLabelRecord
                        {
                            label = label,
                            detailed_label = classDataArr.Length <= 1 ? null : classDataArr.Last()
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                //Print for now
                Console.WriteLine(ex.Message);
            }

            return records;
        }

        /// <summary> 
        /// Method to compute hashes in bulk.
        /// </summary>
        protected async Task<List<HashingResult>>ComputeHashes(List<(RawLogRecord, RawLabelRecord)> rawInput)
        {
            var tasks = new List<Task<HashingResult>>();
            rawInput.ForEach(input => tasks.Add(ComputeHash(input)));

            var results = await Task.WhenAll(tasks);

            return results.ToList();
        }

        /// <summary> 
        /// Method to compute an indivual hash.
        /// </summary>
        protected async Task<HashingResult> ComputeHash((RawLogRecord, RawLabelRecord) rawInput)
        {
            var hasher = new HashExtractor();
            var (log, label) = rawInput;
            var json = JsonConvert.SerializeObject(log);
            var hash = hasher.GetHash(json);
            return new HashingResult(hash, label);
        }

        /// <summary> 
        /// Method to write clean data to dataset file.
        /// </summary>
        protected async Task WriteFile(bool append, List<HashingResult> hashes)
        {
            using (var fw = new StreamWriter(_outputFilePath, append))
            {
                using (var cw = new CsvWriter(fw, CultureInfo.InvariantCulture))
                {
                    await cw.WriteRecordsAsync(hashes);
                    cw.Flush();
                    Console.WriteLine($"{_outputFilePath} Successfully written to local storage \n");
                }
            }
        }

        protected string MapMulticlassLabel(string label)
        {
            switch (label.ToLower())
            {
                case "port-scan": return "2";
                case "c&c": return "3";
                case "dos": return "4";
                default: throw new InvalidDataException();
            }
        }

        protected bool MapBinaryLabel(string label)
        {
            switch (label.ToLower())
            {
                case "benign": return false;
                case "port-scan": return true;
                case "c&c": return true;
                case "dos": return true;
                case "malicious": return true;
                default: throw new InvalidDataException();
            }
        }

        #endregion
    }

    /// <summary> 
    /// Class that holds intermediate values before they are mapped to the model input
    /// </summary>
    internal class Log
    {

        public string Hash { get; set; }
        public string Label { get; set; }
    }
}

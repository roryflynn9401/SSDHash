using HashAnalyser.Data;
using HashAnalyser.Prediction;
using Newtonsoft.Json;
using SSDHash;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace LogAnalyser.Processing
{
    public class LogPredictionPipeline
    {
        private HashClassificationPredictor _classificationPredictor = new();
        private HashExtractor _hashExtractor = new();

        public LogPredictionPipeline() { }

        public async Task ProcessLogs(string fileInput)
        {
            //batch for larger files
            string data = File.ReadAllText(fileInput);

            Datatype? dataType = _hashExtractor.IsValidXml(data) ? Datatype.XML : 
                                    _hashExtractor.IsValidJson(data) ?  Datatype.JSON : null;

            object[]? objData = Array.Empty<object>();

            if(dataType == Datatype.JSON)
            {
                 objData = ParseJsonData(data, Datatype.JSON);

            }
            if (objData is null) return;

            string[] strObj = new string[objData.Length];

            for(int i = 0; i < objData.Length; i++) 
            {
                strObj[i] = JsonConvert.SerializeObject(objData[i]);
            }
            var hashes = await ComputeHashes(strObj);

            if(hashes is null) return;

            var binaryResults = _classificationPredictor.PredictBinary(hashes);
            if(binaryResults is null) return;

            var multiclassResults = _classificationPredictor.PredictMulticlass(binaryResults);
            if(multiclassResults is null) return;



        }

        private void Cluster() 
        {
            
        }

        private async Task<string[]> ComputeHashes(string[] inputs)
        {
            var tasks = new List<Task<string>>();

            foreach(var input in inputs)
            {
                tasks.Add(ComputeHash(input));
            }


            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToArray();

        }

        private async Task<string> ComputeHash(string input) => _hashExtractor.GetHash(input) ?? string.Empty;
        

        private object[]? ParseJsonData(string data, Datatype dataType)
        {
            try
            {
                object[]? splitData = JsonConvert.DeserializeObject<object[]>(data);
                if(splitData is null)
                {
                    Console.WriteLine("Invalid JSON data detected.  ");
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
    }
    public enum Datatype
    {
        JSON = 1,
        XML = 2,
    }
}

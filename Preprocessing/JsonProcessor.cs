using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SSDHash.Preprocessing
{
    public class JsonProcessor : IDataProcessor
    {
        public Dictionary<string, string>? Convert(string input)
        {
            var schemaObject = JObject.Parse(input);
            if (schemaObject is null) return null;

            var values = schemaObject
                .SelectTokens("$..*")
                .Where(t => !t.HasValues)
                .ToDictionary(t => t.Path, t => t.ToString());

            return values; 
        }
    }
}

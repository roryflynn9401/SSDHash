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

            var properties = schemaObject.Properties();

            var values = schemaObject
                .SelectTokens("$..*")
                .Where(t => !t.HasValues);
            values = values.Where(t => !IsRemovalableDataType(t));

            var returnValues = values.ToDictionary(t => t.Path.Replace('.', '-'), t => t.ToString());
            return returnValues; 
        }

        internal bool IsRemovalableDataType(JToken token)
        {
            return token.Type switch
            {
                JTokenType.Date => true,
                JTokenType.Guid => true,
                _ => false
            };
        }
    }
}

using Newtonsoft.Json.Linq;
using SpookilySharp;
using System.Text;

string input1 = """{"timestamp": "2023-12-11T08:30:45.123Z","message": "User 'john_doe' successfully logged in."}""";
string input2 = """{"timestamp": "2023-12-11T08:30:45.123Z","message": "This is a completely different message"}""";

var hash1 = GetHash(input1);
var hash2 = GetHash(input2);

var dissimilarity = CalculateDissimilarity(hash1, hash2);

Console.WriteLine($"Hash1: {hash1}");
Console.WriteLine($"Hash2: {hash2}");
Console.WriteLine($"Dissimilarity: {dissimilarity}");

static string GetHash(string input)
{
    var flatInput = FlattenJson(input);

    var tokenizedInput = TokenizeJson(flatInput);

    var prependedInput = PrependFieldNames(tokenizedInput);

    var hashBuckets = HashFieldValues(prependedInput);

    var bucketCounts = CountBuckets(hashBuckets.Values.SelectMany(x => x).ToArray());

    var scaledCounts = ScaleAndQuantize(bucketCounts, 15);

    var hash = GenerateHashDigest(scaledCounts);

    return hash;
}

static Dictionary<string,string> FlattenJson(string json)
{
    var schemaObject = JObject.Parse(json);
    var values = schemaObject
        .SelectTokens("$..*")
        .Where(t => !t.HasValues)
        .ToDictionary(t => t.Path, t => t.ToString());

    return values;
}

static Dictionary<string, string[]> TokenizeJson(Dictionary<string, string> input)
{
    var dict = new Dictionary<string, string[]>();

    foreach(var item in input)
    {
        var tokenizedValues = item.Value.Split(new char[] { ' ', '.', '?' }, StringSplitOptions.RemoveEmptyEntries);
        dict.Add(item.Key, tokenizedValues);
    }

    return dict;
}

static Dictionary<string, string[]> PrependFieldNames(Dictionary<string, string[]> fields)
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

static Dictionary<string, int[]> HashFieldValues(Dictionary<string, string[]> values)
{
    var hashBuckets = new Dictionary<string, int[]>();

    foreach (var entry in values)
    {
        var bucketValues = new int[entry.Value.Length];
        int i = 0;
        foreach(var value in entry.Value)
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

unsafe static ulong Hash64(byte[] data, int length, ulong seed)
{
    fixed (byte* pMessage = data)
    {
        return SpookyHash.Hash64(pMessage, length, seed);
    }
}

static Dictionary<int, int> CountBuckets(int[] buckets)
{
    Dictionary<int, int> bucketCounts = new Dictionary<int, int>();

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

static Dictionary<int, int> ScaleAndQuantize(Dictionary<int, int> bucketCounts, int levels)
{
    int totalOccurrences = bucketCounts.Values.Sum();

    Dictionary<int, double> scaledCounts = bucketCounts.ToDictionary(
        kvp => kvp.Key,
        kvp => (double)kvp.Value / totalOccurrences * levels
    );

    var sortedCounts = scaledCounts.OrderByDescending(x => x.Value).ToList();
    Dictionary<int, int> quantizedCounts = new Dictionary<int, int>();

    for (int i = 0; i < sortedCounts.Count; i++)
    {
        int bucket = sortedCounts[i].Key;
        int rank = (int)Math.Round((double)i / (sortedCounts.Count - 1) * (levels - 1));
        quantizedCounts[bucket] = rank;
    }

    return quantizedCounts;
}

static string GenerateHashDigest(Dictionary<int, int> quantizedCounts)
{
    StringBuilder hashDigest = new StringBuilder();

    for(int i = 0; i < 64; i++)
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

static double CalculateDissimilarity(string jsonHash1, string jsonHash2)
{
    Func<int, int> Hexcp2i = cp =>
    {
        if (cp >= 97) return cp - 87;
        else return cp - 48;
    };

    var xlen = jsonHash2.Length;
    var z = Enumerable.Range(0, xlen);
    var x = jsonHash2.Select(c => (int)c);
    var y = jsonHash1.Select(c => (int)c);

    var result = z
        .Select(zVal =>
        {
            var xi = Hexcp2i(x.ElementAt(zVal));
            var yi = Hexcp2i(y.ElementAt(zVal));

            var dmax = xi > yi ? xi : yi;
            var dmin = xi < yi ? xi : yi;

            return new { dmax, dmin };
        })
        .GroupBy(item => true) //needs better solution for performing aggregate
        .Select(group =>
        {
            var maxsum = group.Sum(item => item.dmax);
            var minsum = group.Sum(item => item.dmin);

            return 1.0 - (double)minsum / maxsum;
        })
        .FirstOrDefault();

    return result;
}
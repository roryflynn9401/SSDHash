using Newtonsoft.Json.Linq;
using SpookilySharp;
using SSDHash.Preprocessing;
using System.Text;

public class Program
{
    private static List<string> FileNames = new();

    public static void Main(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help":
                    HelpMenu();
                    break;
                case "-i":
                case "-j":
                default:
                    var isParam = (arg == "-i" || arg == "-j") && i < args.Length;
                    var filePath = Path.GetFullPath(isParam ? args[i + 1] : arg);

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        FileNames.Add(filePath);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command or file does not exist");
                        HelpMenu();
                    }

                    if(isParam) i++;
                    break;
            }
        }
        if(FileNames.Count != 2)
        {
            Console.WriteLine($"Insufficient number of files provided. Files provides {FileNames.Count}");
            HelpMenu();
        }

        var file1Contents = GetFileContents(FileNames[0]);
        var file2Contents = GetFileContents(FileNames[1]);

        if(file1Contents is null || file2Contents is null)
        {
            Console.WriteLine("Error reading file contents");
            return;
        }

        var hashExtractor = new HashExtractor();

        var hash1 = hashExtractor.GetHash(file1Contents);
        var hash2 = hashExtractor.GetHash(file2Contents);

        if(hash1 is null || hash2 is null)
        {
            Console.WriteLine("Error generating hash. Incorrect or invalid data format.");
            return;
        }

        var dissimilarity = CalculateDissimilarity(hash1, hash2);

        Console.WriteLine($"""
            Hash (i) : {hash1}
            Hash (j) : {hash2}
            --------------------------------
            Dissimilarity : {dissimilarity * 100}%
            """);
        
    }
    #region File access

    private static string? GetFileContents(string fileName)
    {
        try
        {
            using (var fs = File.OpenRead(fileName))
            {
                var sb = new StringBuilder();
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        sb.Append(sr.ReadLine());
                    }
                }
                return sb.ToString();
            }
        }
        catch(FileNotFoundException)
        {
            Console.WriteLine($"File {fileName} not found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {fileName}. Error: {ex.Message}");
        }
        return null;
    }

    #endregion

    #region Dissimilarity Calculations

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
            });

        var sumDmax = result.Sum(r => r.dmax);
        var sumDmin = result.Sum(r => r.dmin);

        var dissim = 1 - (double)sumDmin / sumDmax;

        return dissim;
    }

    #endregion

    #region Menu Prints
    
    private static void HelpMenu()
    {
        Console.WriteLine($"""
            This is the help menu for SSDHash. Below are the command-line arguments available:
            -h|--help : Help Manu,


            """);
    }

    #endregion
}
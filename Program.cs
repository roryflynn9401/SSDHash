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
                case "-h":
                case "--help":
                    HelpMenu();
                    break;
                default:
                    if (File.Exists(arg))
                    {
                        FileNames.Add(arg);
                    }
                    else
                    {
                        Console.WriteLine("Invalid command or file does not exist");
                        HelpMenu();
                    }
                    break;
            }
        }
        if(FileNames.Count != 2)
        {
            Console.WriteLine($"Insufficient number of files provided. Files provides {FileNames.Count}");
            HelpMenu();
        }



        var hashExtractor = new HashExtractor();
        
    }

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
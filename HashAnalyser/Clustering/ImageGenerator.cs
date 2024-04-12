using Microsoft.ML;
using ScottPlot;
using ScottPlot.DataSources;
using SSDHash;

namespace HashAnalyser.Clustering
{
    public class ImageGenerator
    {
        public class HashSimilarity
        {
            public double Dissimilarity { get; set; }
        }

        public void ShowDissimClusters(string[] hashes)
        {
            double[] similarity = new double[hashes.Length];

            for(int i  =0; i < similarity.Length; i++)
            {
                similarity[i]  = HashProcessing.CalculateDissimilarity("050000000004020ED0A730A0010056010600C400009000000C000802080D00B9", hashes[i]);
            }
            
            var ml = new MLContext(0);

            var pipeline = ml.Transforms.NormalizeMeanVariance("Dissimilarity", "Dissimilarity");
            var input = similarity.Select(x => new HashSimilarity { Dissimilarity = x }).ToArray();
            var dv = ml.Data.LoadFromEnumerable(input);

            var model = pipeline.Fit(dv);
            var results = model.Transform(dv);

            var norms = ml.Data.CreateEnumerable<HashSimilarity>(results, false).ToArray();

            Plot plot = new();
            var splotArr = new ScatterSourceDoubleArray(Enumerable.Range(0, norms.Length).Select(x => (double)x).ToArray(), norms.Select(x => x.Dissimilarity).ToArray());
            plot.Add.ScatterPoints(splotArr);
            

            plot.SaveJpeg("Scatterplot.jpg", 3960, 2160);
        }

        public void ShowClusters(Dictionary<uint, List<(double X, double Y)>> points)
        {
            var colors = new[] { "B31E1E", "E0FF00", "0023FF", "000000", "761EB3", "FFFFFF", "ABBAAB", "012090", "123456", "999999"};
            Plot plot = new();
            var count = 0;

            foreach(var coords in points)
            {
                var xCoords = coords.Value.Select(x => x.X).ToArray();
                var yCoords = coords.Value.Select(x => x.Y).ToArray();
                var splotArr = new ScatterSourceDoubleArray(xCoords, yCoords);

                plot.Add.ScatterPoints(splotArr, Color.FromHex(colors[count]));
                count++;
            }

            plot.SaveJpeg("KMeansClusters.jpg", 1920, 1080);
        }
    }
}
                        
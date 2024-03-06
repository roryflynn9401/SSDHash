using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDHash
{
    public static class HashProcessing
    {
        public static double CalculateDissimilarity(string hash1, string hash2)
        {
            Func<int, int> Hexcp2i = cp =>
            {
                if (cp >= 97) return cp - 87;
                else return cp - 48;
            };

            var xlen = hash2.Length;
            var z = Enumerable.Range(0, xlen);
            var x = hash2.Select(c => (int)c);
            var y = hash1.Select(c => (int)c);

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
    }
}

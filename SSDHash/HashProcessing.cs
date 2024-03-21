using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDHash
{
    public static class HashProcessing
    {
        public static double CalculateJaccardDissimilarity(string hash1, string hash2)
        {
            Func<int, int> Hexcp2i = cp =>
            {
                if (cp >= 97) return cp - 87;
                else return cp - 48;
            };

            var xlen = hash2.Length;

           var intersection = hash1.Intersect(hash2).Count();
           var union = hash1.Union(hash2).Count();

            var dissim = (double)intersection/ union;
            return dissim;
        }
    }
}

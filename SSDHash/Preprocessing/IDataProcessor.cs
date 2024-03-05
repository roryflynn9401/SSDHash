using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDHash.Preprocessing
{
    public interface IDataProcessor
    {
        Dictionary<string, string>? Convert(string input);
    }
}

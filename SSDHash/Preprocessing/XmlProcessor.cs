using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SSDHash.Preprocessing
{
    public class XmlProcessor : IDataProcessor
    {
        public Dictionary<string, string>? Convert(string input)
        {
            try
            {
                var xDocument = XDocument.Parse(input);

                var values = xDocument.Descendants()
                    .Where(e => !e.HasElements)
                    .ToDictionary(e => GetElementPath(e), e => e.Value);

                return values;
            }
            catch (Exception)
            {
                // Invalid XML
                return null; 
            }
        }

        private string GetElementPath(XElement? element)
        {
            var elements = new List<XElement>();
            while (element != null)
            {
                elements.Add(element);
                element = element.Parent;
            }

            return string.Join(".", elements.Select(e => e.Name.LocalName));
        }
    }
}

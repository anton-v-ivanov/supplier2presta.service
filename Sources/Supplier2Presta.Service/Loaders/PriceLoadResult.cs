using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class PriceLoadResult
    {
        public PriceLoadResultType Result { get; private set; }
        public List<string> PriceLines { get; private set; }
        public string FilePath { get; private set; }

        public PriceLoadResult(List<string> priceLines, string filePath, PriceLoadResultType result)
        {
            PriceLines = priceLines;
            FilePath = filePath;
            Result = result;
        }

        public PriceLoadResult(List<string> priceLines, PriceLoadResultType result)
        {
            PriceLines = priceLines;
            Result = result;
        }
    }
}

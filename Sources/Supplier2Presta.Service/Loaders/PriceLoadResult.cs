using Supplier2Presta.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class PriceLoadResult
    {
        public bool Success { get; set; }
        public Dictionary<string, PriceItem> PriceItems { get; private set; }
        public string FilePath { get; private set; }

        public PriceLoadResult(Dictionary<string, PriceItem> priceItems, string filePath, bool success)
        {
            PriceItems = priceItems;
            FilePath = filePath;
            Success = success;
        }

        public PriceLoadResult(Dictionary<string, PriceItem> priceItems, bool success)
        {
            PriceItems = priceItems;
            Success = success;
        }
    }
}

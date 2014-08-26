using System.Collections.Generic;

using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.Diffs
{
    public class Diff
    {
        public Dictionary<string, PriceItem> NewItems { get; set; }
        
        public Dictionary<string, PriceItem> DeletedItems { get; set; }
        
        public Dictionary<string, PriceItem> UpdatedItems { get; set; }
    }
}
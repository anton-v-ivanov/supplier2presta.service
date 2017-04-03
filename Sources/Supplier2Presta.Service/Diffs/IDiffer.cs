using Supplier2Presta.Service.Entities;
using System.Collections.Generic;

namespace Supplier2Presta.Service.Diffs
{
    public interface IDiffer
    {
    Diff GetDiff(Dictionary<string, PriceItem> newProds, Dictionary<string, PriceItem> oldProds, IEnumerable<string> ignoredProducts);
    }
}
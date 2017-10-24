using System.Collections.Generic;
using Supplier2Presta.Entities;

namespace Supplier2Presta.Diffs
{
    public interface IDiffer
    {
        Diff GetDiff(Dictionary<string, PriceItem> newProds, Dictionary<string, PriceItem> oldProds, IEnumerable<string> ignoredProducts);
    }
}
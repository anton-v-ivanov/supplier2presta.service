using System;
using System.Collections.Generic;
using System.Linq;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.Diffs
{
    public class Differ : IDiffer
    {
        public Diff GetDiff(Dictionary<string, PriceItem> newProds, Dictionary<string, PriceItem> oldProds, IEnumerable<string> ignoredProducts)
        {
            oldProds = oldProds ?? new Dictionary<string, PriceItem>();

            var diff = new Diff 
            { 
                NewItems = newProds.Where(kvp => !oldProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                DeletedItems = oldProds.Where(kvp => !newProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                UpdatedItems = newProds.Where(kvp => oldProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value)
            };

            var toRemove = new List<string>();
            foreach (var item in diff.UpdatedItems.Values)
            {
                var oldItem = oldProds[item.Reference];
                if (item.WholesalePrice.Equals(oldItem.WholesalePrice) && item.Active == oldItem.Active && !SameBalance(item, oldItem))
                {
                    toRemove.Add(item.Reference);
                }
            }

            foreach (var reference in toRemove)
            {
                diff.UpdatedItems.Remove(reference);
            }

			foreach (var ignoredProduct in ignoredProducts)
			{
				diff.NewItems.Remove(ignoredProduct);
				diff.UpdatedItems.Remove(ignoredProduct);
			}

			return diff;
        }

        private bool SameBalance(PriceItem item, PriceItem oldItem)
        {
            foreach (var assort in item.Assort)
            {
                var oldAssort = oldItem.Assort
                    .FirstOrDefault(s => s.Size.Equals(assort.Size, StringComparison.OrdinalIgnoreCase) && 
                        s.Color.Equals(assort.Color, StringComparison.OrdinalIgnoreCase));
                
                if(oldAssort != null && oldAssort.Balance != assort.Balance)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
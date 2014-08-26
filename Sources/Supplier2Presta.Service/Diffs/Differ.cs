using System.Collections.Generic;
using System.Linq;

using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service.Diffs
{
    public class Differ : IDiffer
    {
        private readonly IPriceItemBuilder builder;

        public Differ(IPriceItemBuilder builder)
        {
            this.builder = builder;
        }

        public Diff GetDiff(List<string> newLines, List<string> oldLines)
        {
            var localLines = newLines.Skip(1).ToList(); // пропуск строки с заголовками
            
            var newItems = localLines.Select(this.builder.Build);
            
            var newProds = new Dictionary<string, PriceItem>();
            foreach (var item in newItems)
            {
                if (!newProds.ContainsKey(item.Reference))
                {
                    newProds.Add(item.Reference, item);
                }
            }

            var oldProds = new Dictionary<string, PriceItem>();
            if (oldLines != null)
            {
                localLines = oldLines.Skip(1).ToList(); // пропуск строки с заголовками

                var oldItems = localLines.Select(this.builder.Build);
                foreach (var item in oldItems)
                {
                    if (!oldProds.ContainsKey(item.Reference))
                    {
                        oldProds.Add(item.Reference, item);
                    }
                }
            }

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
                if (item.RetailPrice == oldItem.RetailPrice && item.WholesalePrice == oldItem.WholesalePrice 
                    && item.Active == oldItem.Active && item.Balance == oldItem.Balance)
                {
                    toRemove.Add(item.Reference);
                }
            }

            foreach (var reference in toRemove)
            {
                diff.UpdatedItems.Remove(reference);
            }

            return diff;
        }
    }
}
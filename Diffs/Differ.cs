using System.Collections.Generic;
using System.Linq;

using Supplier2Presta.Entities;
using Supplier2Presta.PriceItemBuilders;

namespace Supplier2Presta.Diffs
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
            
            var newItems = localLines.Select(builder.Build);
            
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

                var oldItems = localLines.Select(builder.Build);
                foreach (var item in oldItems)
                {
                    if (!oldProds.ContainsKey(item.Reference))
                    {
                        oldProds.Add(item.Reference, item);
                    }
                }
            }

            return new Diff 
            { 
                NewItems = newProds.Where(kvp => !oldProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                DeletedItems = oldProds.Where(kvp => !newProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                SameItems = oldProds.Where(kvp => newProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value) 
            };
        }
    }
}
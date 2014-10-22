using System.Collections.Generic;
using System.Linq;

using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service.Diffs
{
    public class Differ : IDiffer
    {
        public Diff GetDiff(Dictionary<string, PriceItem> newProds, Dictionary<string, PriceItem> oldProds, bool forceUpdate)
        {
            oldProds = oldProds ?? new Dictionary<string, PriceItem>();

            var diff = new Diff 
            { 
                NewItems = newProds.Where(kvp => !oldProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                DeletedItems = oldProds.Where(kvp => !newProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value),
                UpdatedItems = newProds.Where(kvp => oldProds.ContainsKey(kvp.Key)).ToDictionary(key => key.Key, value => value.Value)
            };

            // if force update flag is set, than we shold keep all the data in updated field
            if (!forceUpdate)
            {
                var toRemove = new List<string>();
                foreach (var item in diff.UpdatedItems.Values)
                {
                    var oldItem = oldProds[item.Reference];
                    if (item.WholesalePrice == oldItem.WholesalePrice && item.Active == oldItem.Active && item.Balance == oldItem.Balance)
                    {
                        toRemove.Add(item.Reference);
                    }
                }

                foreach (var reference in toRemove)
                {
                    diff.UpdatedItems.Remove(reference);
                }
            }

            return diff;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Supplier2Presta.Entities;

namespace Supplier2Presta.Config
{
    public class StartArgs
    {
        public bool IsForce { get; }
        public IEnumerable<PriceType> UpdateTypes { get; }
        public IEnumerable<string> DebugReferences { get; }

        public StartArgs(bool isForce, IEnumerable<PriceType> updateTypes, IEnumerable<string> debugReferences)
        {
            IsForce = isForce;
            UpdateTypes = updateTypes;
            DebugReferences = debugReferences;
        }

        public static StartArgs FromArgs(string[] args)
        {
            var updateTypes = new List<PriceType>();
            List<string> debugReferences = null;

            var forceUpdate = false;
            if (args != null)
            {
                forceUpdate = args.Any(s => s.Equals("force", StringComparison.OrdinalIgnoreCase));

                var debugRefStr = args.FirstOrDefault(s => s.StartsWith("r:", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(debugRefStr))
                    debugReferences = debugRefStr.Replace("r:", string.Empty).Split(',').ToList();

                var typeStr = args.FirstOrDefault(s => s.StartsWith("t:", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(typeStr))
                {
                    var typesStr = typeStr.Replace("t:", string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var type in typesStr)
                    {
                        if (type.Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            updateTypes.Clear();
                            updateTypes.AddRange(Enum.GetValues(typeof(PriceType)).Cast<PriceType>());
                            break;
                        }

                        var priceType = (PriceType)Enum.Parse(typeof(PriceType), type, true);
                        if (!updateTypes.Contains(priceType))
                            updateTypes.Add(priceType);
                    }
                }
                else
                {
                    updateTypes.Add(PriceType.Stock);
                }
            }

            Log.Information("Update price types: {0}", string.Join(",", updateTypes.ToArray()));

            return new StartArgs(forceUpdate, updateTypes, debugReferences);
        }
    }
}
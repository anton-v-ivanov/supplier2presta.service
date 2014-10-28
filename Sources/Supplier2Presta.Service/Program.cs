using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using NLog;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.PriceItemBuilders;
using Supplier2Presta.Service.Processors;
using System.Globalization;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Managers;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.PriceBuilders;

namespace Supplier2Presta.Service
{
    public class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static int Main(string[] args)
        {
            Log.Info("Price update started");

            List<PriceType> updateTypes = new List<PriceType>();
            var forceUpdate = false;
            if (args != null)
            {
                forceUpdate = args.Any(s => s.Equals("forceUpdate", StringComparison.OrdinalIgnoreCase));
                var typeStr = args.FirstOrDefault(s => s.StartsWith("t:", StringComparison.OrdinalIgnoreCase));
                if(!string.IsNullOrWhiteSpace(typeStr))
                {
                    var typesStr = typeStr.Replace("t:", string.Empty).Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var type in typesStr)
	                {
                        if(type.Equals("all", StringComparison.OrdinalIgnoreCase))
                        {
                            updateTypes.Clear();
                            updateTypes.AddRange(Enum.GetValues(typeof(PriceType)).Cast<PriceType>());
                            break;
                        }

                        var priceType = (PriceType)Enum.Parse(typeof(PriceType), type, true);
                        if(!updateTypes.Contains(priceType))
                            updateTypes.Add(priceType);
	                }
                }
                else
                {
                    updateTypes.Add(PriceType.Stock);
                }
            }

            Log.Info("Update price types: {0}", string.Join(",", updateTypes.ToArray()));

            var apiUrl = ConfigurationManager.AppSettings["api-url"];
            var apiAccessToken = ConfigurationManager.AppSettings["api-access-token"];

            foreach (SupplierElement settings in RobotSettings.Config.Suppliers)
            {
                if (updateTypes.Contains(settings.PriceType))
                {
                    var priceManager = PriceManagerBuilder.Build(settings, apiUrl, apiAccessToken);

                    Log.Info("Processing price: '{0}' Type: {1}", settings.Name, settings.PriceType);

                    var result = priceManager.CheckUpdates(settings.PriceType, forceUpdate);
                    if (result.Status != PriceUpdateResultStatus.Ok && result.Status != PriceUpdateResultStatus.ProcessAborted)
                    {
                        Log.Info("Price update finished. ErrorCode: " + result);
                        return Convert.ToInt32(result);
                    }
                }
            }
            
            return Convert.ToInt32(PriceUpdateResultStatus.Ok);
        }
    }
}

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
using Supplier2Presta.Service.Multiplicators;

namespace Supplier2Presta.Service
{
    public class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IPriceManager _priceManager;
        private static string _newPriceUrl;
        private static string _fullPriceUrl;

        static Program()
        {
            try
            {
                var settings = RobotSettings.Config.Suppliers[0];
            
                _newPriceUrl = settings.StockPriceUrl;
                _fullPriceUrl = settings.FullPriceUrl;

                var priceEncoding = settings.PriceEncoding;

			    var archiveDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), settings.ArchiveDirectory);
            
                var apiUrl = ConfigurationManager.AppSettings["api-url"];
                var apiAccessToken = ConfigurationManager.AppSettings["api-access-token"];

                IRetailPriceBuilder retailPriceBuilder = new RetailPriceBuilder(settings.Multiplicators);

                switch (Path.GetExtension(_newPriceUrl))
                {
                    case ".xml":
                        _priceManager = new HappinesXmlPriceManager(archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken);
                        break;
                
                    case ".csv":
                        _priceManager = new HappinesCsvPriceManager(priceEncoding, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken);
                        break;
                
                    default:
                        throw new ConfigurationException("There is no price manager for file with extension " + Path.GetExtension(_newPriceUrl));
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
                throw;
            }

        }

        public static int Main(string[] args)
        {
            Log.Info("Price update started");
            var result = _priceManager.CheckProductsUpdates(_newPriceUrl);
            if (result.Status != PriceUpdateResultStatus.Ok && result.Status != PriceUpdateResultStatus.ProcessAborted)
            {
				Log.Info("Price update finished. ErrorCode: " + result);
                return Convert.ToInt32(result);
            }

            var forceFull = args != null && args.Count() > 0 && args[0] == "full";
            if (!result.HasNewProducts && !forceFull)
            {
				Log.Info("Price update finished");
                return Convert.ToInt32(result.Status);
            }

            if (result.HasNewProducts) 
			{
				Log.Info("There are new products is price");
			}
			if (forceFull) 
			{
				Log.Info("Full price update forced");
			}

            result = _priceManager.CheckNewProducts(_fullPriceUrl);
			Log.Info("Price update finished");
			return Convert.ToInt32(result.Status);
        }
    }
}

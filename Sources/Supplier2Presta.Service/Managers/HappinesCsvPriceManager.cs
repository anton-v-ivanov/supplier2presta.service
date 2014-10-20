using NLog;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.Multiplicators;
using Supplier2Presta.Service.PriceItemBuilders;
using Supplier2Presta.Service.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Supplier2Presta.Service.Managers
{
    public class HappinesCsvPriceManager : PriceManagerBase, IPriceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private string _priceEncoding;
        private IDiffer _differ;
        private string _apiUrl;
        private string _apiAccessToken;

        public HappinesCsvPriceManager(string priceEncoding, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
            : base(archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
            _priceEncoding = priceEncoding;
            _apiUrl = apiUrl;
            _apiAccessToken = apiAccessToken;
            
            _differ = new Differ();
        }

        public PriceUpdateResult CheckProductsUpdates(string priceUrl)
        {
            var priceFormat = GetPriceFormat("happiness_short.xml");

            var csvPriceLoader = new CsvPriceLoader(_priceEncoding, priceFormat);
            var oldPriceLoader = new NewestFileSystemPriceLoader(csvPriceLoader);
            var newPriceLoader = new InternetPriceLoader(csvPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<string>(priceUrl);
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            var oldPriceLoadResult = oldPriceLoader.Load<string>(_archiveDirectory);

            return base.ProcessShortPrice(newPriceLoadResult, oldPriceLoadResult);
        }

        public PriceUpdateResult CheckNewProducts(string priceUrl)
        {
            Log.Debug("Loading full price from supplier site");
            var priceFormat = GetPriceFormat("happiness.xml");

            var csvPriceLoader = new CsvPriceLoader(_priceEncoding, priceFormat);
            var oldPriceLoader = new NewestFileSystemPriceLoader(csvPriceLoader);
            var newPriceLoader = new InternetPriceLoader(csvPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<string>(priceUrl);
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }
            
            var archiveDir = Path.Combine(_archiveDirectory, "full");
            var oldPriceLoadResult = oldPriceLoader.Load<string>(archiveDir);

            return base.ProcessFullPrice(newPriceLoadResult, oldPriceLoadResult, archiveDir);
        }

        private PriceFormat GetPriceFormat(string priceFormatFile)
        {
            var serializer = new XmlSerializer(typeof(PriceFormat));
            PriceFormat priceFormat;
            var priceFormatFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), priceFormatFile);
            using (var stream = new FileStream(priceFormatFilePath, FileMode.Open))
            {
                priceFormat = (PriceFormat)serializer.Deserialize(stream);
            }
            return priceFormat;
        }
    }
}

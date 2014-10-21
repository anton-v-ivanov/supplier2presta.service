using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.Multiplicators;
using System;
using System.IO;

namespace Supplier2Presta.Service.Managers
{
    public class HappinesXmlPriceManager : PriceManagerBase, IPriceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public HappinesXmlPriceManager(string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
            : base(archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
            _archiveDirectory = archiveDirectory;
        }

        public PriceUpdateResult CheckProductsUpdates(string priceUrl)
        {
            var xmlPriceLoader = new XmlPriceLoader();
            var oldPriceLoader = new NewestFileSystemPriceLoader(xmlPriceLoader);
            var newPriceLoader = new SingleFilePriceLoader(xmlPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(priceUrl);
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            var oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(_archiveDirectory);

            return base.ProcessShortPrice(newPriceLoadResult, oldPriceLoadResult);
        }

        public PriceUpdateResult CheckNewProducts(string priceUrl)
        {
            Log.Debug("Loading full price from supplier site");

            var xmlPriceLoader = new XmlPriceLoader();
            var oldPriceLoader = new NewestFileSystemPriceLoader(xmlPriceLoader);
            var newPriceLoader = new SingleFilePriceLoader(xmlPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(priceUrl);
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            var archiveDir = Path.Combine(_archiveDirectory, "full");
            var oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(archiveDir);

            return base.ProcessFullPrice(newPriceLoadResult, oldPriceLoadResult, archiveDir);
        }
    }
}

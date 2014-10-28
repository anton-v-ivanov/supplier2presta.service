using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using System;
using System.IO;

namespace Supplier2Presta.Service.Managers
{
    public class HappinesXmlPriceManager : PriceManagerBase, IPriceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public HappinesXmlPriceManager(SupplierElement settings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
            : base(settings, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
        }

        public PriceUpdateResult CheckUpdates(PriceType type, bool forceUpdate)
        {
            var xmlPriceLoader = new XmlPriceLoader();
            var oldPriceLoader = new NewestFileSystemPriceLoader(xmlPriceLoader);
            var newPriceLoader = new SingleFilePriceLoader(xmlPriceLoader);

            PriceLoadResult newPriceLoadResult;
            PriceLoadResult oldPriceLoadResult;
            switch (type)
            {
                case PriceType.Stock:
                    newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(_settings.Url);
                    oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(_archiveDirectory);
                    break;

                case PriceType.Full:
                    newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(_settings.Url);
                    oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(_archiveDirectory);
                    break;

                case PriceType.Discount:
                    throw new NotImplementedException();

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            
            return base.Process(newPriceLoadResult, oldPriceLoadResult, type, forceUpdate);
        }
    }
}

using System;
using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service.Managers
{
    public class HappinesXmlPriceManager : PriceManagerBase, IPriceManager
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IColorBuilder _colorCodeBuilder;

        public HappinesXmlPriceManager(SupplierElement settings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken, IColorBuilder colorCodeBuilder)
            : base(settings, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
            _colorCodeBuilder = colorCodeBuilder;
        }

        public PriceUpdateResult CheckUpdates(PriceType type, bool forceUpdate)
        {
            var xmlPriceLoader = new XmlPriceLoader(_colorCodeBuilder);
            var oldPriceLoader = new NewestFileSystemPriceLoader(xmlPriceLoader);
            var newPriceLoader = new SingleFilePriceLoader(xmlPriceLoader);

            PriceLoadResult newPriceLoadResult;
            PriceLoadResult oldPriceLoadResult = null;
            switch (type)
            {
                case PriceType.Stock:
                    newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(Settings.Url);
                    if (!forceUpdate)
                    {
                        oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(ArchiveDirectory);
                    }
                    break;

                case PriceType.Full:
                    newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(Settings.Url);
                    if (!forceUpdate)
                    {
                        oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(ArchiveDirectory);
                    }
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
                        
            return Process(newPriceLoadResult, oldPriceLoadResult, type);
        }
    }
}

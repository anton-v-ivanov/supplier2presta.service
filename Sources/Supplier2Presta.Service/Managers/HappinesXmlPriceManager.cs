using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.PriceItemBuilders;
using System;
using System.IO;

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
                    newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(_settings.Url);
                    if (!forceUpdate)
                    {
                        oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(_archiveDirectory);
                    }
                    break;

                case PriceType.Full:
                    newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(_settings.Url);
                    if (!forceUpdate)
                    {
                        oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(_archiveDirectory);
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
                        
            return base.Process(newPriceLoadResult, oldPriceLoadResult, type);
        }
    }
}

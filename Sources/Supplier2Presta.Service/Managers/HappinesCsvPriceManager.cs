using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.PriceItemBuilders;
using Supplier2Presta.Service.ShopApiProcessors;
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

        public HappinesCsvPriceManager(SupplierElement settings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
            : base(settings, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
        }

        public PriceUpdateResult CheckUpdates(PriceType type, bool forceUpdate)
        {
            var priceFormat = GetPriceFormat(_settings.PriceFormatFile);
            
            var csvPriceLoader = new CsvPriceLoader(_settings.PriceEncoding, priceFormat);
            var newPriceLoader = new SingleFilePriceLoader(csvPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<string>(_settings.Url);
            
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            PriceLoadResult oldPriceLoadResult = null;

            if (!forceUpdate)
            {
                var oldPriceLoader = new NewestFileSystemPriceLoader(csvPriceLoader);
                oldPriceLoadResult = oldPriceLoader.Load<string>(_archiveDirectory);
            }

            return base.Process(newPriceLoadResult, oldPriceLoadResult, type);
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

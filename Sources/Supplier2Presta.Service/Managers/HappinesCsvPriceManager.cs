using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;

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
            var priceFormat = GetPriceFormat(Settings.PriceFormatFile);
            
            var csvPriceLoader = new CsvPriceLoader(Settings.PriceEncoding, priceFormat);
            var newPriceLoader = new SingleFilePriceLoader(csvPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<string>(Settings.Url);
            
            if (!newPriceLoadResult.Success)
            {
                Log.Fatal("Unable to load new price");
                return new PriceUpdateResult(PriceUpdateResultStatus.PriceLoadFail);
            }

            PriceLoadResult oldPriceLoadResult = null;

            if (!forceUpdate)
            {
                var oldPriceLoader = new NewestFileSystemPriceLoader(csvPriceLoader);
                oldPriceLoadResult = oldPriceLoader.Load<string>(ArchiveDirectory);
            }

            return Process(newPriceLoadResult, oldPriceLoadResult, type);
        }

        private PriceFormat GetPriceFormat(string priceFormatFile)
        {
            var serializer = new XmlSerializer(typeof(PriceFormat));
            PriceFormat priceFormat;
            var priceFormatFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), priceFormatFile);
            using (var stream = new FileStream(priceFormatFilePath, FileMode.Open))
            {
                priceFormat = (PriceFormat)serializer.Deserialize(stream);
            }
            return priceFormat;
        }
    }
}

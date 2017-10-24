using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Supplier2Presta.Config;
using Supplier2Presta.Entities;
using Supplier2Presta.Loaders;
using Supplier2Presta.PriceBuilders;
using Supplier2Presta.ShopApiProcessors;

namespace Supplier2Presta.Managers
{
    public class HappinesCsvPriceManager : PriceManagerBase
    {
        private readonly string _priceFormatFile;
        private readonly string _priceEncoding;

        public HappinesCsvPriceManager(IProcessor processor, SupplierSettings supplierSettings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder)
            : base(processor, supplierSettings, archiveDirectory, retailPriceBuilder)
        {
            _priceFormatFile = supplierSettings.FormatFile;
            _priceEncoding = supplierSettings.Encoding;
        }

        public override LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate)
        {
            var priceFormat = GetPriceFormat(_priceFormatFile);
            
            var csvPriceLoader = new CsvPriceLoader(_priceEncoding, priceFormat);
            var newPriceLoader = new SingleFilePriceLoader(csvPriceLoader);

            var newPriceLoadResult = newPriceLoader.Load<string>(PriceUrl);
            
            PriceLoadResult oldPriceLoadResult = null;

            if (!forceUpdate)
            {
                var oldPriceLoader = new NewestFileSystemPriceLoader(csvPriceLoader);
                oldPriceLoadResult = oldPriceLoader.Load<string>(ArchiveDirectory);
            }

            return new LoadUpdatesResult(newPriceLoadResult, oldPriceLoadResult, newPriceLoadResult.Success);
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

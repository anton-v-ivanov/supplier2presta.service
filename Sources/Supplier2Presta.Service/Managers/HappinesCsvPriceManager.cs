using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;

namespace Supplier2Presta.Service.Managers
{
    public class HappinesCsvPriceManager : PriceManagerBase
    {
        private readonly string _priceFormatFile;
        private readonly string _priceEncoding;

        public HappinesCsvPriceManager(string priceUrl, int discountValue, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken, string priceFormatFile, string priceEncoding)
            : base(priceUrl, discountValue, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
        {
            _priceFormatFile = priceFormatFile;
            _priceEncoding = priceEncoding;
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

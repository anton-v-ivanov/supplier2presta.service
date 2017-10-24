using System.IO;
using System.Linq;

namespace Supplier2Presta.Loaders
{
    public class NewestFileSystemPriceLoader : IPriceLoader
    {
        private readonly IPriceLoader _internalLoader;

        public NewestFileSystemPriceLoader(IPriceLoader priceLoader)
        {
            _internalLoader = priceLoader;
        }
        
        public PriceLoadResult Load<T>(string uri)
        {
            var directory = new DirectoryInfo(uri);
            if (!directory.Exists)
            {
                directory.Create();
            }

            var ext = typeof(T).Name.Contains("XmlItemList") ? "*.xml" : "*.csv";
            var oldPriceFile = directory.GetFiles(ext)
             .OrderByDescending(f => f.LastWriteTime)
             .FirstOrDefault();
                        
            if (oldPriceFile != null)
            {
                return _internalLoader.Load<T>(oldPriceFile.FullName);
            }

            return new PriceLoadResult(null, false);
        }
    }
}

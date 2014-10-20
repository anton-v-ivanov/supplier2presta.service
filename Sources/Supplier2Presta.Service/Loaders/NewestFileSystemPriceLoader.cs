using NLog;
using Supplier2Presta.Service.Entities.XmlPrice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class NewestFileSystemPriceLoader : IPriceLoader
    {
        private IPriceLoader _internalLoader;

        public NewestFileSystemPriceLoader(IPriceLoader priceLoader)
        {
            _internalLoader = priceLoader;
        }
        
        public PriceLoadResult Load<T>(string uri)
        {
            var directory = new DirectoryInfo(uri);
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

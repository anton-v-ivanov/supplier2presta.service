using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class NewestFileSystemPriceLoader : PriceLoaderDecoratorBase
    {
        public override PriceLoadResult Load(string uri, string encoding)
        {
            var directory = new DirectoryInfo(uri);
            var oldPriceFile = directory.GetFiles()
             .OrderByDescending(f => f.LastWriteTime)
             .FirstOrDefault();

            
            if (oldPriceFile != null)
            {
                return base.Load(oldPriceFile.FullName, encoding);
            }

            return new PriceLoadResult(null, PriceLoadResultType.PriceFileIsNotExists);
        }
    }
}

using Supplier2Presta.Service.Entities.XmlPrice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class SingleFilePriceLoader : IPriceLoader
    {
        private IPriceLoader _internalLoader;

        public SingleFilePriceLoader(IPriceLoader priceLoader)
        {
            _internalLoader = priceLoader;
        }
        
        public PriceLoadResult Load<T>(string uri)
        {
            var ext = Path.GetExtension(uri);
            var newPriceFileName = string.Format("price_{0}{1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), ext);
            if (uri.StartsWith("http"))
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(uri, newPriceFileName);
                }
            }
            else
            {
                File.Copy(uri, newPriceFileName);
            }

            return _internalLoader.Load<T>(newPriceFileName);
        }
    }
}

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
    public class InternetPriceLoader : IPriceLoader
    {
        private IPriceLoader _internalLoader;

        public InternetPriceLoader(IPriceLoader priceLoader)
        {
            _internalLoader = priceLoader;
        }
        
        public PriceLoadResult Load<T>(string uri)
        {
            var ext = Path.GetExtension(uri);
            var newPriceFileName = string.Format("price_{0}{1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), ext);
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(uri, newPriceFileName);
            }
            
            return _internalLoader.Load<T>(newPriceFileName);
        }
    }
}

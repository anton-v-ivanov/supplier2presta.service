using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Supplier2Presta.Service.Loaders
{
    public class SingleFilePriceLoader : IPriceLoader
    {
        private readonly IPriceLoader _internalLoader;

        public SingleFilePriceLoader(IPriceLoader priceLoader)
        {
            _internalLoader = priceLoader;
        }
        
        public PriceLoadResult Load<T>(string uri)
        {
            var ext = Path.GetExtension(uri);
            var newPriceFileName = string.Format("price_{0}{1}", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"), ext);
            newPriceFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), newPriceFileName);
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

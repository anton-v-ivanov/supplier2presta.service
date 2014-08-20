using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Loaders
{
    public class InternetPriceLoader : PriceLoaderDecoratorBase
    {
        public override PriceLoadResult Load(string uri, string encoding)
        {
            var newPrice = new List<string>();
            var newPriceFileName = string.Format("price_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(uri, newPriceFileName);
            }
            
            return base.Load(newPriceFileName, encoding);
        }
    }
}

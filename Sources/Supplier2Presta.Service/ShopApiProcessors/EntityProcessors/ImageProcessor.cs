using Bukimedia.PrestaSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class ImageProcessor
    {
        private readonly ShopApiFactory _apiFactory;

        public ImageProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        internal image GetImageValue(string url, product product)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (var client = new WebClient())
            {
                try
                {
                    var bytes = client.DownloadData(url);
                    return _apiFactory.ImageFactory.AddProductImage(product.id.Value, bytes);
                }
                catch (Exception)
                {
                    //Log.Error("Error while loading product image", ex);
                    return null;
                }
            }
        }
    }
}

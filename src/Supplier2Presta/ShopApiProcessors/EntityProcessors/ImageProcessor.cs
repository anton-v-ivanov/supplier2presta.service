using System;
using System.Net;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public class ImageProcessor: IImageProcessor
    {
        private readonly IShopApiFactory _apiFactory;

        public ImageProcessor(IShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        public async Task<image> GetImageValue(string url, product product)
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
                    return await _apiFactory.ImageFactory.AddProductImage(product.id.Value, bytes);
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

using Bukimedia.PrestaSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.ShopApiProcessors
{
    public class ProductRemover
    {
        private readonly ShopApiFactory _apiFactory;

        public ProductRemover(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        public void Remove(product product)
        {
            product.active = 0;
            _apiFactory.ProductFactory.Update(product);
        }
    }
}

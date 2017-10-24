using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public class SupplierProcessor: ISupplierProcessor
    {
        private readonly IShopApiFactory _apiFactory;

        public SupplierProcessor(IShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        public async Task<product_supplier> GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var supplier = (await _apiFactory.ProductSupplierFactory.GetByFilter(filter, null, null)).FirstOrDefault();
            if (supplier != null)
                return supplier;

            supplier = new product_supplier
            {
                id_currency = 1,
                id_product = product.id,
                id_product_attribute = 0,
                id_supplier = supplierFeature.id,
                product_supplier_reference = priceItem.SupplierReference,
                product_supplier_price_te = Convert.ToDecimal(priceItem.WholesalePrice)
            };
            supplier = await _apiFactory.ProductSupplierFactory.Add(supplier);

            return supplier;
        }
    }
}

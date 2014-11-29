using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class SupplierProcessor
    {
        private readonly ShopApiFactory _apiFactory;

        public SupplierProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        internal product_supplier GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var supplier = _apiFactory.ProductSupplierFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (supplier == null)
            {
                supplier = new product_supplier
                {
                    id_currency = 1,
                    id_product = product.id,
                    id_product_attribute = 0,
                    id_supplier = supplierFeature.id,
                    product_supplier_reference = priceItem.SupplierReference,
                    product_supplier_price_te = Convert.ToDecimal(priceItem.WholesalePrice)
                };
                supplier = _apiFactory.ProductSupplierFactory.Add(supplier);
            }

            return supplier;
        }
    }
}

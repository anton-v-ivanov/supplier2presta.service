using System;
using System.Collections.Generic;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class CategoryProcessor
    {
        private readonly ShopApiFactory _apiFactory;

        public CategoryProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        internal category GetCategoryValue(PriceItem priceItem)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Category } };

            var category = _apiFactory.CategoryFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (category == null)
            {
                throw new Exception(string.Format("Unknown category: {0}", priceItem.Category));
            }
            return category;
        }
    }
}

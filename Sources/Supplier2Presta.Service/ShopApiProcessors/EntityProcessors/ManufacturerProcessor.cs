using System.Collections.Generic;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class ManufacturerProcessor
    {
        private readonly ShopApiFactory _apiFactory;

        public ManufacturerProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        internal manufacturer GetManufacturerValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Manufacturer } };
            var manufacturers = _apiFactory.ManufacturerFactory.GetByFilter(filter, null, null);

            if (manufacturers == null || !manufacturers.Any())
            {
                var manufacturer = new manufacturer
                {
                    name = priceItem.Manufacturer,
                    active = 1,
                };
                return _apiFactory.ManufacturerFactory.Add(manufacturer);
            }
            return manufacturers.First();
        }
    }
}

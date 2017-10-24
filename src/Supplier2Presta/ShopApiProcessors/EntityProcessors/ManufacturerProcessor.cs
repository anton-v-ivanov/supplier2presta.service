using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public class ManufacturerProcessor: IManufacturerProcessor
    {
        private readonly IShopApiFactory _apiFactory;

        public ManufacturerProcessor(IShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        public async Task<manufacturer> GetManufacturerValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Manufacturer } };
            var manufacturers = await _apiFactory.ManufacturerFactory.GetByFilter(filter, null, null);

            if (manufacturers == null || !manufacturers.Any())
            {
                var manufacturer = new manufacturer
                {
                    name = priceItem.Manufacturer,
                    active = 1,
                };
                return await _apiFactory.ManufacturerFactory.Add(manufacturer);
            }
            return manufacturers.First();
        }
    }
}

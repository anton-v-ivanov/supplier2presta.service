using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using NLog;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class StockProcessor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ShopApiFactory _apiFactory;
        private readonly CombinationsProcessor _combinationProcessor;

        public StockProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _combinationProcessor = new CombinationsProcessor(apiFactory);
        }

        internal void UpdateStockValue(PriceItem priceItem, product product)
        {
            foreach (var assort in priceItem.Assort)
            {
                var stock = GetStockValue(product, assort);
	            if (stock.quantity == assort.Balance)
					continue;

				stock.quantity = assort.Balance;
	            _apiFactory.StockFactory.Update(stock);
	            Log.Info("Balance changed from {0} to {1}. Reference: {2}", stock.quantity, assort.Balance, priceItem.Reference);
            }
        }

        private stock_available GetStockValue(product product, Assort assort)
        {
            combination combination = null;
            if (!string.IsNullOrWhiteSpace(assort.Size) || !string.IsNullOrWhiteSpace(assort.Color) || !string.IsNullOrWhiteSpace(assort.Reference))
            {
                combination = _combinationProcessor.GetOrCreateCombination(product, assort);
            }

            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var stocks = _apiFactory.StockFactory.GetByFilter(filter, null, null);
            if (stocks == null || !stocks.Any())
            {
                return CreateStock(product, assort, combination);
            }

            var stock = combination != null 
                ? stocks.FirstOrDefault(s => s.id_product_attribute == combination.id) 
                : stocks.FirstOrDefault(s => s.id_product == product.id);

            return stock ?? CreateStock(product, assort, combination);
        }

        private stock_available CreateStock(product product, Assort assort, combination combination)
        {
            var stock = new stock_available
            {
                id_product = product.id,
                quantity = assort.Balance
            };
            if (combination != null)
            {
                stock.id_product_attribute = combination.id;
            }

            return _apiFactory.StockFactory.AddList(new List<stock_available> { stock }).First();
        }
    }
}

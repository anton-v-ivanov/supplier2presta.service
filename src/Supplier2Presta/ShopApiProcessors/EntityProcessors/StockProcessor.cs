using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Serilog;
using Supplier2Presta.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public class StockProcessor: IStockProcessor
    {
        private readonly IShopApiFactory _apiFactory;
        private readonly ICombinationsProcessor _combinationProcessor;

        public StockProcessor(IShopApiFactory apiFactory, ICombinationsProcessor combinationsProcessor)
        {
            _apiFactory = apiFactory;
            _combinationProcessor = combinationsProcessor;
        }

        public async Task UpdateStockValue(PriceItem priceItem, product product)
        {
            foreach (var assort in priceItem.Assort)
            {
                var stock = await GetStockValue(product, assort);
	            if (stock.quantity == assort.Balance)
					continue;

                var prevQuantity = stock.quantity;
				stock.quantity = assort.Balance;
	            await _apiFactory.StockFactory.Update(stock);
	            Log.Information("Balance changed from {quantity} to {Balance}. Reference: {Reference}", prevQuantity, stock.quantity, priceItem.Reference);
            }
        }

        private async Task<stock_available> GetStockValue(product product, Assort assort)
        {
            combination combination = null;
            if (!string.IsNullOrWhiteSpace(assort.Size) || !string.IsNullOrWhiteSpace(assort.Color) || !string.IsNullOrWhiteSpace(assort.Reference))
            {
                combination = await _combinationProcessor.GetOrCreateCombination(product, assort);
            }

            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var stocks = await _apiFactory.StockFactory.GetByFilter(filter, null, null);
            if (stocks == null || !stocks.Any())
            {
                return await CreateStock(product, assort, combination);
            }

            var stock = combination != null 
                ? stocks.FirstOrDefault(s => s.id_product_attribute == combination.id) 
                : stocks.FirstOrDefault(s => s.id_product == product.id);

            return stock ?? await CreateStock(product, assort, combination);
        }

        private async Task<stock_available> CreateStock(product product, Assort assort, combination combination)
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

            return (await _apiFactory.StockFactory.AddList(new List<stock_available> { stock })).First();
        }
    }
}

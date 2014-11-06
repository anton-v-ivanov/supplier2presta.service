using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Service.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class StockProcessor
    {
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
                var stock = this.GetStockValue(product, assort);
                if (stock.quantity != assort.Balance)
                {
                    stock.quantity = assort.Balance;
                    _apiFactory.StockFactory.Update(stock);
                }
            }
        }

        private stock_available GetStockValue(product product, Assort assort)
        {
            combination combination = null;
            if (!string.IsNullOrWhiteSpace(assort.Size) || !string.IsNullOrWhiteSpace(assort.Color) || !string.IsNullOrWhiteSpace(assort.Reference))
            {
                combination = _combinationProcessor.GetCombination(product, assort);
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
            else
            {
                stock_available stock;
                if (combination != null)
                {
                    stock = stocks.FirstOrDefault(s => s.id_product_attribute == combination.id);
                }
                else
                {
                    stock = stocks.FirstOrDefault(s => s.id_product == product.id);
                }

                if(stock == null)
                {
                    return CreateStock(product, assort, combination);
                }
                return stock;
            }
        }

        private stock_available CreateStock(product product, Assort assort, Bukimedia.PrestaSharp.Entities.combination combination)
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

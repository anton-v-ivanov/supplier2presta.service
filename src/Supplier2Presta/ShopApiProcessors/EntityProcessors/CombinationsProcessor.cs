using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Entities.AuxEntities;
using Serilog;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;
using product = Bukimedia.PrestaSharp.Entities.product;
using product_option_value = Bukimedia.PrestaSharp.Entities.product_option_value;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    internal class CombinationsProcessor: ICombinationsProcessor
    {
        private readonly IShopApiFactory _apiFactory;

		public CombinationsProcessor(IShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        public async Task FillOptions(PriceItem priceItem, product product)
        {
            foreach (var assort in priceItem.Assort)
            {
                await GetOrCreateCombination(product, assort);
            }
        }

        public async Task<combination> GetOrCreateCombination(product product, Assort assort)
        {
            Dictionary<string, string> filter;
            List<combination> combinations = null;
            if (!string.IsNullOrWhiteSpace(assort.Reference))
            {
                filter = new Dictionary<string, string> { { "reference", assort.Reference } };
                combinations = await _apiFactory.CombinationFactory.GetByFilter(filter, null, null);
            }

            if(combinations != null && combinations.Any())
            {
                return combinations.First();
            }

            product_option_value colorOptionValue = null;
            if (!string.IsNullOrWhiteSpace(assort.Color))
            {
                colorOptionValue = await GetOptionValue(assort.Color, assort.ColorCode, _apiFactory.ColorOption.id.Value);
            }

            product_option_value sizeOptionValue = null;
            if (!string.IsNullOrWhiteSpace(assort.Size))
            {
                sizeOptionValue = await GetOptionValue(assort.Size, string.Empty, _apiFactory.SizeOption.id.Value);
            }

            if (colorOptionValue == null && sizeOptionValue == null)
            {
                return null;
            }

            filter = new Dictionary<string, string> { { "id_product", product.id.Value.ToString() } };
            combinations = await _apiFactory.CombinationFactory.GetByFilter(filter, null, null);
            if (combinations == null || !combinations.Any())
            {
                return await CreateCombination(product, assort, sizeOptionValue, colorOptionValue, true);
            }

	        foreach (var combination in combinations)
	        {
		        if (sizeOptionValue != null && colorOptionValue != null)
		        {
			        if (combination.associations.product_option_values.Exists(s => s.id == sizeOptionValue.id.Value) &&
			            combination.associations.product_option_values.Exists(s => s.id == colorOptionValue.id.Value))
			        {
				        return await CheckCombination(combination, assort, product);
			        }
		        }
		        else if (colorOptionValue != null)
		        {
			        if (combination.associations.product_option_values.Exists(s => s.id == colorOptionValue.id.Value))
			        {
				        return await CheckCombination(combination, assort, product);
			        }
		        }
		        else
		        {
			        if (combination.associations.product_option_values.Exists(s => s.id == sizeOptionValue.id.Value))
			        {
				        return await CheckCombination(combination, assort, product);
			        }
		        }
	        }

	        return await CreateCombination(product, assort, sizeOptionValue, colorOptionValue, false);
        }

        private async Task<combination> CheckCombination(combination combination, Assort assort, product product)
        {
            if (combination != null && combination.reference != assort.Reference)
            {
                combination.reference = assort.Reference;
                await _apiFactory.CombinationFactory.Update(combination);
            }
            
            if(!product.associations.combinations.Exists(s => s.id == combination.id))
            {
                product = ProductsMapper.MapCombination(product, combination);
                await _apiFactory.ProductFactory.Update(product);
            }
            return combination;
        }

        private async Task<combination> CreateCombination(product product, Assort assort, product_option_value sizeOptionValue, product_option_value colorOptionValue, bool isDefault)
        {
			var combination = new combination
            {
                id_product = product.id,
                reference = assort.Reference,
                ean13 = assort.Ean13,
                associations = new AssociationsCombination(),
                minimal_quantity = 1,
                default_on = Convert.ToInt32(isDefault),
            };
            combination.associations.product_option_values = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.product_option_value>();
            if (colorOptionValue != null)
            {
                combination.associations.product_option_values.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.product_option_value { id = colorOptionValue.id.Value });
            }
            if(sizeOptionValue != null)
            {
                combination.associations.product_option_values.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.product_option_value { id = sizeOptionValue.id.Value });
            }

            combination = await _apiFactory.CombinationFactory.Add(combination);
            product = ProductsMapper.MapCombination(product, combination);

			Log.Information("Combitation created. Size: {0}, Color: {1}. Product reference: {2}, Combination Reference: {3}",
		        sizeOptionValue?.name[0].Value,
		        colorOptionValue?.name[0].Value,
		        product.reference,
		        combination.reference);

			await _apiFactory.ProductFactory.Update(product);
            return combination;
        }

        private async Task<product_option_value> GetOptionValue(string option, string colorCode, long optionId)
        {
            var filter = new Dictionary<string, string> { { "name", option } };
            var optionValue = (await _apiFactory.OptionsValueFactory.GetByFilter(filter, null, null)).FirstOrDefault();
            if (optionValue == null)
            {
                optionValue = new product_option_value
                {
                    name = new List<language> { new language(1, option) },
                    id_attribute_group = optionId,
                    color = colorCode,
                };
                return await _apiFactory.OptionsValueFactory.Add(optionValue);
            }
            return optionValue;
        }
    }
}

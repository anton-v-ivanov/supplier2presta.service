using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Serilog;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;
using Supplier2Presta.ShopApiProcessors.EntityProcessors;

namespace Supplier2Presta.ShopApiProcessors
{
	public class ProductUpdater: IProductUpdater
    {
		private readonly IShopApiFactory _apiFactory;
		private readonly IStockProcessor _stockProcessor;

		public ProductUpdater(IShopApiFactory apiFactory, IStockProcessor stockProcessor)
		{
		    _apiFactory = apiFactory;
		    _stockProcessor = stockProcessor;
		}

		public async Task Update(product product, PriceItem item, PriceType processingPriceType)
		{
			await _stockProcessor.UpdateStockValue(item, product);
			await UpdateMetaInfo(item, product);
		    await UpdatePhotos(item, product);
			if (processingPriceType == PriceType.Stock || processingPriceType == PriceType.Full)
			{
				UpdateProductPriceAndActivity(item, product);
			}
			if (processingPriceType == PriceType.Discount)
			{
				await UpdateDiscountInfo(item, product);
			}
		}

        private async Task UpdatePhotos(PriceItem item, product product)
        {
            if (product.id_default_image == null && product.associations.images.Any())
            {
                var imageId = product.associations.images.First().id;
                Log.Information("Product's default photo is null, setting default photo. Reference: {reference}, ImageId: {id}", item.Reference, imageId);
                product.id_default_image = imageId;
                await _apiFactory.ProductFactory.Update(product);
            }
        }

        private void UpdateProductPriceAndActivity(PriceItem item, product product)
		{
			// price of onSale products is updated by special file
			if (product.on_sale != 0 ||
			    product.active == Convert.ToInt32(item.Active) && product.price == Convert.ToDecimal(item.RetailPrice) &&
			    product.wholesale_price == Convert.ToDecimal(item.WholesalePrice))
				return;

			var prevPrice = product.price;
			var prevWholesalePrice = product.wholesale_price;

			product.active = Convert.ToInt32(item.Active);
			product.price = Convert.ToDecimal(item.RetailPrice);
			product.wholesale_price = Convert.ToDecimal(item.WholesalePrice);
			Log.Debug("Updating price. Reference: {0}", item.Reference);
			_apiFactory.ProductFactory.Update(product);
			if (prevPrice != product.price)
				Log.Information("Price changed from {prevPrice} to {price}. Reference: {reference}", prevPrice.ToString("##.###"), product.price.ToString("##.###"), item.Reference);
			if (prevWholesalePrice != product.wholesale_price)
				Log.Information("Wholesale price changed from {prevPrice} to {price}. Reference: {reference}", prevWholesalePrice.ToString("##.###"), product.wholesale_price.ToString("##.###"), item.Reference);
		}

		private async Task UpdateMetaInfo(PriceItem item, product product)
		{
			if (!SameMetaInfo(item, product))
			{
				product = ProductsMapper.FillMetaInfo(item, product);
				if (product.on_sale == 1)
				{
					var specialPriceRule = await GetSpecialPriceRule(product);
					if (specialPriceRule != null)
					{
						if (specialPriceRule.reduction_type != "percentage")
						{
							throw new NotImplementedException();
						}

						product.price = product.price / specialPriceRule.reduction;
					}
				}
				Log.Debug("Updating meta info. Reference: {0}", item.Reference);
				await _apiFactory.ProductFactory.Update(product);
				Log.Information("Meta info updated. Reference: {0}", item.Reference);
			}
		}

		public async Task UpdateDiscountInfo(PriceItem item, product product)
		{
			specific_price specialPriceRule = null;

			if (product.on_sale == 1)
				specialPriceRule = await GetSpecialPriceRule(product);

			if ((product.on_sale == 1 || item.OnSale) && product.on_sale != Convert.ToInt32(item.OnSale))
			{
				if (specialPriceRule != null)
				{
					if (!item.OnSale && product.on_sale == 1)
					{
						// remove special price
						Log.Information("Removing discount info. Reference: {0}", item.Reference);
						await _apiFactory.SpecialPriceFactory.Delete(specialPriceRule);
					}
					else
					{
						if (specialPriceRule.reduction != Convert.ToDecimal(item.DiscountValue) / 100)
						{
							specialPriceRule.reduction = Convert.ToDecimal(item.DiscountValue) / 100;
							Log.Information("Updating reduction info. Reference: {0}", item.Reference);
							await _apiFactory.SpecialPriceFactory.Update(specialPriceRule);
						}
					}
				}
				else
				{
					specialPriceRule = new specific_price
					{
						id_product = product.id,
						reduction = Convert.ToDecimal(item.DiscountValue) / 100,
						reduction_type = "percentage",
						id_shop = 1,
						id_cart = 0,
						id_currency = 0,
						id_country = 0,
						id_group = 0,
						id_customer = 0,
						from_quantity = 1,
						price = -1,
					};
					Log.Information("Adding discount price info. Reference: {0}", item.Reference);
					await _apiFactory.SpecialPriceFactory.Add(specialPriceRule);
				}
			}

			var productRetailPrice = Convert.ToDecimal(item.RetailPrice);
			if (specialPriceRule != null)
			{
				productRetailPrice = Math.Ceiling(productRetailPrice * specialPriceRule.reduction);
			}

			if (product.on_sale != Convert.ToInt32(item.OnSale) ||
				product.price != productRetailPrice ||
				product.wholesale_price != Convert.ToDecimal(item.WholesalePrice))
			{
				product.on_sale = Convert.ToInt32(item.OnSale);
				product.price = Convert.ToDecimal(item.RetailPrice);
				product.wholesale_price = Convert.ToDecimal(item.WholesalePrice);
				Log.Debug("Updating discount info. Reference: {0}", item.Reference);
				await _apiFactory.ProductFactory.Update(product);
			}
		}

		public async Task RemoveDiscountInfo(PriceItem item, product product)
		{
			var specialPriceRule = await GetSpecialPriceRule(product);
			if (specialPriceRule != null)
			{
				Log.Information("Removing special price. Reference: {0}", item.Reference);
				await _apiFactory.SpecialPriceFactory.Delete(specialPriceRule);
			}

			if (product.on_sale != 1)
				return;

			product.on_sale = 0;
			product.price = Convert.ToDecimal(item.RetailPrice);
			Log.Debug("Removing discount. Reference: {0}", item.Reference);
			await _apiFactory.ProductFactory.Update(product);
		}

		private async Task<specific_price> GetSpecialPriceRule(product product)
		{
			var filter = new Dictionary<string, string> { { "id_product", Convert.ToString(product.id) } };
			var specialPriceRule = (await _apiFactory.SpecialPriceFactory.GetByFilter(filter, null, null)).FirstOrDefault();
			return specialPriceRule;
		}

		private static bool SameMetaInfo(PriceItem priceItem, product product)
		{
			if (string.IsNullOrWhiteSpace(priceItem.Name))
				return true;

			if (product.meta_title == null ||
				!product.meta_title.Any() ||
				!product.meta_title[0].Value.Equals(priceItem.Name, StringComparison.OrdinalIgnoreCase))
				return false;

			if (product.meta_description == null ||
				!product.meta_description.Any() ||
				!product.meta_description[0].Value.Equals($"Купить {priceItem.Name} дёшево с доставкой", StringComparison.OrdinalIgnoreCase))
				return false;

			if (product.meta_keywords == null ||
				!product.meta_keywords.Any())
				return false;


			if (!product.meta_keywords.Exists(s => s.Value.Equals($"Купить {priceItem.Name} дёшево с доставкой", StringComparison.OrdinalIgnoreCase)))
				return false;

			return true;
		}
	}
}

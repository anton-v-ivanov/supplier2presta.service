using System;
using System.Collections.Generic;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.ShopApiProcessors.EntityProcessors;

namespace Supplier2Presta.Service.ShopApiProcessors
{
    public class ProductUpdater
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly ShopApiFactory _apiFactory;
        private readonly StockProcessor _stockProcessor;

        public ProductUpdater(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _stockProcessor = new StockProcessor(apiFactory);
        }

        public void Update(product product, PriceItem item, PriceType processingPriceType)
        {
            _stockProcessor.UpdateStockValue(item, product);
            UpdateMetaInfo(item, product);
            if (processingPriceType == PriceType.Stock)
            {
                UpdateProductPriceAndActivity(item, product);
            }
            if (processingPriceType == PriceType.Discount)
            {
                UpdateDiscountInfo(item, product);
            }
        }

        private void UpdateProductPriceAndActivity(PriceItem item, product product)
        {
            // price of onSale products is updated by special file
            if (product.on_sale == 0 &&
                (product.active != Convert.ToInt32(item.Active) ||
                product.price != Convert.ToDecimal(item.RetailPrice) || 
                product.wholesale_price != Convert.ToDecimal(item.WholesalePrice)))
            {
                product.active = Convert.ToInt32(item.Active);
                product.price = Convert.ToDecimal(item.RetailPrice);
                product.wholesale_price = Convert.ToDecimal(item.WholesalePrice);
                Log.Debug("Updating price. Reference: {0}", item.Reference);
                _apiFactory.ProductFactory.Update(product);
            }
        }

        private void UpdateMetaInfo(PriceItem item, product product)
        {
            if (!SameMetaInfo(item, product))
            {
                product = ProductsMapper.FillMetaInfo(item, product);
				if (product.on_sale == 1) 
				{
					var specialPriceRule = GetSpecialPriceRule(product);
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
                _apiFactory.ProductFactory.Update(product);
            }
        }

        public void UpdateDiscountInfo(PriceItem item, product product)
        {
			specific_price specialPriceRule = null;

			if (product.on_sale == 1)
				specialPriceRule = GetSpecialPriceRule(product);

			if ((product.on_sale == 1 || item.OnSale) && product.on_sale != Convert.ToInt32(item.OnSale))
            {
                if (specialPriceRule != null)
                {
                    if (!item.OnSale && product.on_sale == 1)
                    {
                        // remove special price
                        Log.Info("Removing discount info. Reference: {0}", item.Reference);
                        _apiFactory.SpecialPriceFactory.Delete(specialPriceRule);
                    }
                    else
                    {
                        if (specialPriceRule.reduction != Convert.ToDecimal(item.DiscountValue) / 100)
                        {
                            specialPriceRule.reduction = Convert.ToDecimal(item.DiscountValue) / 100;
                            Log.Info("Updating reduction info. Reference: {0}", item.Reference);
                            _apiFactory.SpecialPriceFactory.Update(specialPriceRule);
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
                    Log.Info("Adding discount price info. Reference: {0}", item.Reference);
                    _apiFactory.SpecialPriceFactory.Add(specialPriceRule);
                }
            }

			var productRetailPrice = Convert.ToDecimal(item.RetailPrice);
			if (specialPriceRule != null) 
			{
				productRetailPrice = Math.Ceiling(productRetailPrice * specialPriceRule.reduction);
			}

            if (product.on_sale != Convert.ToInt32(item.OnSale) ||
				product.price !=  productRetailPrice ||
                product.wholesale_price != Convert.ToDecimal(item.WholesalePrice))
            {
                product.on_sale = Convert.ToInt32(item.OnSale);
                product.price = Convert.ToDecimal(item.RetailPrice);
                product.wholesale_price = Convert.ToDecimal(item.WholesalePrice);
                Log.Debug("Updating discount info. Reference: {0}", item.Reference);
                _apiFactory.ProductFactory.Update(product);
            }
        }

        internal void RemoveDiscountInfo(PriceItem item, product product)
        {
            var specialPriceRule = GetSpecialPriceRule(product);
            if(specialPriceRule != null)
            {
                Log.Info("Removing special price. Reference: {0}", item.Reference);
                _apiFactory.SpecialPriceFactory.Delete(specialPriceRule);
            }

            if (product.on_sale == 1)
            {
                product.on_sale = 0;
                product.price = Convert.ToDecimal(item.RetailPrice);
                Log.Debug("Removing discount. Reference: {0}", item.Reference);
                _apiFactory.ProductFactory.Update(product);
            }
        }

		private specific_price GetSpecialPriceRule(product product)
		{
			var filter = new Dictionary<string, string> { { "id_product", Convert.ToString (product.id) } };
			var specialPriceRule = _apiFactory.SpecialPriceFactory.GetByFilter(filter, null, null).FirstOrDefault();
			return specialPriceRule;
		}

        private bool SameMetaInfo(PriceItem priceItem, product product)
        {
            if (string.IsNullOrWhiteSpace(priceItem.Name))
                return true;

            if (product.meta_title == null ||
                !product.meta_title.Any() ||
                !product.meta_title[0].Value.Equals(priceItem.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            if (product.meta_description == null ||
                !product.meta_description.Any() ||
                !product.meta_description[0].Value.Equals(string.Format("Купить {0} в Москве", priceItem.Name), StringComparison.OrdinalIgnoreCase))
                return false;

            if (product.meta_keywords == null ||
                !product.meta_keywords.Any())
                return false;


            if (!product.meta_keywords.Exists(s => s.Value.Equals(string.Format("Купить {0} в Москве", priceItem.Name), StringComparison.OrdinalIgnoreCase)))
                return false;

            return true;
        }
    }
}

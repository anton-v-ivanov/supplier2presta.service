using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.Entities;

namespace Supplier2Presta.PriceBuilders
{
	public class RetailPriceBuilder : IRetailPriceBuilder
	{
	    private readonly SupplierSettings _settings;

		public RetailPriceBuilder(SupplierSettings settings)
		{
		    _settings = settings;
		}

		public float GetRetailPrice(PriceItem priceItem)
		{
			float price;
		    if (_settings.Multiplicators == null)
		    {
		        price = GetDefaultPrice(priceItem);
                price = (float)Math.Ceiling(price);
                if (Math.Abs(price) < 0.01)
                {
                    Log.Error("Retail price build error. Price is 0. Reference: {reference}", priceItem.Reference);
                    throw new Exception($"Retail price build error. Price is 0. Reference: {priceItem.Reference}");
                }
                return price;
            }

            var setByReferece = TrySetPriceByReference(_settings.Multiplicators, priceItem, out price);
			if(!setByReferece)
			{
				var setByCategory = TrySetPriceByCategory(_settings.Multiplicators, priceItem, out price);
				if(!setByCategory)
				{
					var setByMinMax = TrySetPriceByMinMax(_settings.Multiplicators, priceItem, out price);
					if(!setByMinMax)
					{
						price = GetDefaultPrice(priceItem);
					}
				}
			}

			price = (float)Math.Ceiling(price);
		    if (Math.Abs(price) < 0.01)
		    {
                Log.Error("Retail price build error. Price is 0. Reference: {reference}", priceItem.Reference);
		        throw new Exception($"Retail price build error. Price is 0. Reference: {priceItem.Reference}");
		    }
			return price;
		}

		private float GetDefaultPrice(PriceItem priceItem)
		{
			// 0 means leave supplier recommended price
			if (_settings.DefaultMultiplicator.Equals(0))
			{
				return priceItem.RetailPrice;
			}

			return priceItem.WholesalePrice * _settings.DefaultMultiplicator;
		}

		private static bool TrySetPriceByMinMax(IEnumerable<Multiplicator> elements, PriceItem priceItem, out float price)
		{
			var minMaxPriceElement = elements.FirstOrDefault(e => e.MinPrice <= priceItem.WholesalePrice && priceItem.WholesalePrice <= e.MaxPrice);
			if (minMaxPriceElement != null)
			{
				// 0 means leave supplier recommended price
				if (minMaxPriceElement.Value.Equals(0))
				{
					price = priceItem.RetailPrice;
				}
				else
				{
					price = priceItem.WholesalePrice * minMaxPriceElement.Value;
				}
				return true;
			}
			price = 0;
			return false;
		}

		private static bool TrySetPriceByReference(IEnumerable<Multiplicator> elements, PriceItem priceItem, out float price)
		{
			var referenceElement = elements.FirstOrDefault(e => !string.IsNullOrEmpty(e.ProductReference) && e.ProductReference.Equals(priceItem.Reference, StringComparison.OrdinalIgnoreCase));
			if (referenceElement != null)
			{
				// 0 means leave supplier recommended price
				if (referenceElement.Value.Equals(0))
				{
					price = priceItem.RetailPrice;
				}
				else
				{
					price = priceItem.WholesalePrice * referenceElement.Value;
				}
				return true;
			}
			price = 0;
			return false;
		}

		private static bool TrySetPriceByCategory(IEnumerable<Multiplicator> elements, PriceItem priceItem, out float price)
		{
			var category = priceItem.Categories.FirstOrDefault();
			if (category != null)
			{
				var categoryElement = elements.FirstOrDefault(e => !string.IsNullOrEmpty(e.Category) && 
                    (e.Category.Equals(category.SubName, StringComparison.OrdinalIgnoreCase) ||
				    e.Category.Equals(priceItem.Categories[0].Name, StringComparison.OrdinalIgnoreCase)));

                if (categoryElement != null)
				{
					// 0 means leave supplier recommended price
					if (categoryElement.Value.Equals(0))
					{
						price = priceItem.RetailPrice;
					}
					else
					{
						price = priceItem.WholesalePrice * categoryElement.Value;
					}
					return true;
				}
			}
			price = 0;
			return false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.PriceBuilders
{
	public class RetailPriceBuilder : IRetailPriceBuilder
	{
		private readonly MultiplicatorsElementCollection _multiplicators;

		public RetailPriceBuilder(MultiplicatorsElementCollection multiplicators)
		{
			_multiplicators = multiplicators;
		}

		public float GetRetailPrice(PriceItem priceItem)
		{
			float price;
			var elements = _multiplicators.Cast<MultiplicatorRuleElement>().ToList();

			var setByReferece = TrySetPriceByReference(elements, priceItem, out price);
			if(!setByReferece)
			{
				var setByCategory = TrySetPriceByCategory(elements, priceItem, out price);
				if(!setByCategory)
				{
					var setByMinMax = TrySetPriceByMinMax(elements, priceItem, out price);
					if(!setByMinMax)
					{
						price = GetDefaultPrice(priceItem);
					}
				}
			}

			price = (float)Math.Ceiling(price);
			return price;
		}

		private float GetDefaultPrice(PriceItem priceItem)
		{
			// 0 means leave supplier recommended price
			if (_multiplicators.Default.Equals(0))
			{
				return priceItem.RetailPrice;
			}

			return priceItem.WholesalePrice * _multiplicators.Default;
		}

		private static bool TrySetPriceByMinMax(List<MultiplicatorRuleElement> elements, PriceItem priceItem, out float price)
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

		private static bool TrySetPriceByReference(List<MultiplicatorRuleElement> elements, PriceItem priceItem, out float price)
		{
			var referenceElement = elements.FirstOrDefault(e => e.ProductReference.Equals(priceItem.Reference, StringComparison.OrdinalIgnoreCase));
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

		private static bool TrySetPriceByCategory(List<MultiplicatorRuleElement> elements, PriceItem priceItem, out float price)
		{
			var category = priceItem.Categories.FirstOrDefault();
			if (category != null)
			{
				var categoryElement = elements.FirstOrDefault(e => e.Category.Equals(category.SubName, StringComparison.OrdinalIgnoreCase) ||
												 e.Category.Equals(priceItem.Categories[0].Name, StringComparison.OrdinalIgnoreCase));
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

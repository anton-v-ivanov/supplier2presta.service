using System;
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

		public float Build(PriceItem priceItem)
		{
			float result;
			var elements = _multiplicators.Cast<MultiplicatorRuleElement>().ToList();

			var referenceElement = elements.FirstOrDefault(e => e.ProductReference.Equals(priceItem.Reference, StringComparison.OrdinalIgnoreCase));
			if (referenceElement != null)
			{
				// 0 means leave supplier recommended price
				if (referenceElement.Value == 0)
				{
					result = priceItem.RetailPrice;
				}
				else
				{
					result = priceItem.WholesalePrice * referenceElement.Value;
				}
			}
			else
			{
				var categoryElement = elements.FirstOrDefault(e => e.Category.Equals(priceItem.Categories[0].SubName, StringComparison.OrdinalIgnoreCase) || e.Category.Equals(priceItem.Categories[0].Name, StringComparison.OrdinalIgnoreCase));
				if (categoryElement != null)
				{
					// 0 means leave supplier recommended price
					if (categoryElement.Value == 0)
					{
						result = priceItem.RetailPrice;
					}
					else
					{
						result = priceItem.WholesalePrice * categoryElement.Value;
					}

				}
				else
				{
					var minMaxPriceElement = elements.FirstOrDefault(e => e.MinPrice <= priceItem.WholesalePrice && priceItem.WholesalePrice <= e.MaxPrice);
					if (minMaxPriceElement != null)
					{
						// 0 means leave supplier recommended price
						if (minMaxPriceElement.Value == 0)
						{
							result = priceItem.RetailPrice;
						}
						else
						{
							result = priceItem.WholesalePrice * minMaxPriceElement.Value;
						}
					}
					else
					{
						// 0 means leave supplier recommended price
						if (_multiplicators.Default == 0)
						{
							result = priceItem.RetailPrice;
						}
						else
						{
							result = priceItem.WholesalePrice * _multiplicators.Default;
						}
					}
				}
			}

			result = (float)Math.Ceiling(result);
			return result;
		}
	}
}

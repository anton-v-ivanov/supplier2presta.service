using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Multiplicators
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
            float result = 0;
            var elements = _multiplicators.Cast<MultiplicatorRuleElement>();
            
            var referenceElement = elements.FirstOrDefault(e => e.ProductReference.Equals(priceItem.Reference, StringComparison.OrdinalIgnoreCase));
            if(referenceElement != null)
            {
                return priceItem.WholesalePrice * referenceElement.Value;
            }

            var categoryElement = elements.FirstOrDefault(e => e.Category.Equals(priceItem.Category, StringComparison.OrdinalIgnoreCase) || e.Category.Equals(priceItem.RootCategory, StringComparison.OrdinalIgnoreCase));
            if (categoryElement != null)
            {
                return priceItem.WholesalePrice * categoryElement.Value;
            }

            var minMaxPriceElement = elements.FirstOrDefault(e => e.MinPrice <= priceItem.WholesalePrice && priceItem.WholesalePrice <= e.MaxPrice);
            if (minMaxPriceElement != null)
            {
                return priceItem.WholesalePrice * minMaxPriceElement.Value;
            }

            return result * _multiplicators.Default;
        }
    }
}

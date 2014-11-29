using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
    class FeatureProcessor
    {
        private readonly ShopApiFactory _apiFactory;

        public FeatureProcessor(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        internal product_feature_value GetFeatureValue(string value, product_feature feature)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var filter = new Dictionary<string, string> { { "id_feature", feature.id.Value.ToString(CultureInfo.InvariantCulture) }, { "value", value }, };

            var featureValues = _apiFactory.FeatureValuesFactory.GetByFilter(filter, null, null);
            product_feature_value featureValue;
            if (featureValues == null || !featureValues.Any())
            {
                featureValue = new product_feature_value
                {
                    id_feature = feature.id,
                    value = new List<language> { new language(1, value) }
                };
                featureValue = _apiFactory.FeatureValuesFactory.Add(featureValue);
            }
            else
            {
                featureValue = featureValues.First();
            }

            return featureValue;
        }
    }
}

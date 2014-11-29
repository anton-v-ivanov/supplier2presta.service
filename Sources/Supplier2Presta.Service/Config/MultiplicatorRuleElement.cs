using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed class MultiplicatorRuleElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return ((string)(this["name"]));
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("min-price", IsRequired = false)]
        public float MinPrice
        {
            get
            {
                return ((float)(this["min-price"]));
            }
            set
            {
                this["min-price"] = value;
            }
        }

        [ConfigurationProperty("max-price", IsRequired = false)]
        public float MaxPrice
        {
            get
            {
                return ((float)(this["max-price"]));
            }
            set
            {
                this["max-price"] = value;
            }
        }

        [ConfigurationProperty("product-reference", IsRequired = false)]
        public string ProductReference
        {
            get
            {
                return ((string)(this["product-reference"]));
            }
            set
            {
                this["product-reference"] = value;
            }
        }

        [ConfigurationProperty("category", IsRequired = false)]
        public string Category
        {
            get
            {
                return ((string)(this["category"]));
            }
            set
            {
                this["category"] = value;
            }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public float Value
        {
            get
            {
                var multiplicator = ((float)(this["value"]));
                if (multiplicator < 1 && multiplicator != 0)
                {
                    throw new ConfigurationErrorsException("Multiplicator must be greater than 1");
                }

                return multiplicator;
            }
            set
            {
                this["value"] = value;
            }
        }
    }
}

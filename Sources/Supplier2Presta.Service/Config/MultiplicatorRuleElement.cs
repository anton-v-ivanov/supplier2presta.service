using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed partial class MultiplicatorRuleElement : ConfigurationElement
    {
        [ConfigurationPropertyAttribute("name", IsRequired = true, IsKey = true)]
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

        [ConfigurationPropertyAttribute("min-price", IsRequired = false)]
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

        [ConfigurationPropertyAttribute("max-price", IsRequired = false)]
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

        [ConfigurationPropertyAttribute("product-reference", IsRequired = false)]
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

        [ConfigurationPropertyAttribute("category", IsRequired = false)]
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

        [ConfigurationPropertyAttribute("value", IsRequired = true)]
        public float Value
        {
            get
            {
                return ((float)(this["value"]));
            }
            set
            {
                this["value"] = value;
            }
        }
    }
}

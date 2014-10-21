using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed partial class SupplierElement : ConfigurationElement
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

        [ConfigurationProperty("stock-price-url", IsRequired = true)]
        public string StockPriceUrl
        {
            get
            {
                return ((string)(this["stock-price-url"]));
            }
            set
            {
                this["stock-price-url"] = value;
            }
        }

        [ConfigurationProperty("full-price-url", IsRequired = true)]
        public string FullPriceUrl
        {
            get
            {
                return ((string)(this["full-price-url"]));
            }
            set
            {
                this["full-price-url"] = value;
            }
        }

        [ConfigurationProperty("archive-directory", IsRequired = true)]
        public string ArchiveDirectory
        {
            get
            {
                return ((string)(this["archive-directory"]));
            }
            set
            {
                this["archive-directory"] = value;
            }
        }

        [ConfigurationProperty("price-encoding", IsRequired = false)]
        public string PriceEncoding
        {
            get
            {
                return ((string)(this["price-encoding"] ?? "utf-8"));
            }
            set
            {
                this["price-encoding"] = value;
            }
        }

        [ConfigurationProperty("multiplicators")]
        [ConfigurationCollection(typeof(MultiplicatorRuleElement), AddItemName = "rule")]
        public MultiplicatorsElementCollection Multiplicators
        {
            get
            {
                return ((MultiplicatorsElementCollection)(this["multiplicators"]));
            }
        }
    }
}

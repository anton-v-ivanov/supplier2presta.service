using Supplier2Presta.Service.Entities;
using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed class SupplierElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        [ConfigurationProperty("url", IsRequired = true)]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
        }

        [ConfigurationProperty("supplier", IsRequired = true)]
        public SupplierType Supplier
        {
            get
            {
                return (SupplierType)this["supplier"];
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public PriceType PriceType
        {
            get
            {
                return (PriceType)this["type"];
            }
        }

        [ConfigurationProperty("archive-dir", IsRequired = true)]
        public string ArchiveDirectory
        {
            get
            {
                return (string)this["archive-dir"];
            }
        }

        [ConfigurationProperty("encoding", IsRequired = false)]
        public string PriceEncoding
        {
            get
            {
                return (string)(this["encoding"] ?? "utf-8");
            }
        }

        [ConfigurationProperty("format-file", IsRequired = false)]
        public string PriceFormatFile
        {
            get
            {
                return (string)(this["format-file"]);
            }
        }

        [ConfigurationProperty("discount", IsRequired = false)]
        public int Discount
        {
            get
            {
                return (int)(this["discount"]);
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

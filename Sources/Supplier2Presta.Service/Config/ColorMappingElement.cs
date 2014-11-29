using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public class ColorMappingElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        [ConfigurationProperty("code", IsRequired = true, IsKey = true)]
        public string Code
        {
            get
            {
                return (string)this["code"];
            }
        }
    }
}

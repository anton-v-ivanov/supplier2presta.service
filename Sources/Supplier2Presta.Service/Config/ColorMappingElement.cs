using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

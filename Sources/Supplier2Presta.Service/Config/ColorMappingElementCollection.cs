using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Config
{
    public class ColorMappingElementCollection : ConfigurationElementCollection
    {
        public ColorMappingElement this[int i]
        {
            get
            {
                return ((ColorMappingElement)(this.BaseGet(i)));
            }
        }
        
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ColorMappingElement)(element)).Name;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ColorMappingElement();
        }
    }
}

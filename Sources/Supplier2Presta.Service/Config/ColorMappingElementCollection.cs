using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public class ColorMappingElementCollection : ConfigurationElementCollection
    {
        public ColorMappingElement this[int i]
        {
            get
            {
                return ((ColorMappingElement)(BaseGet(i)));
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

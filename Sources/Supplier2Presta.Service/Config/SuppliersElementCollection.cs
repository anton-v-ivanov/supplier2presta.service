using System.Configuration;
namespace Supplier2Presta.Service.Config
{
    public sealed class SuppliersElementCollection : ConfigurationElementCollection
    {
        public SupplierElement this[int i]
        {
            get
            {
                return ((SupplierElement)(BaseGet(i)));
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SupplierElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SupplierElement)(element)).Name;
        }
    }
}

using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed class RobotSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty("suppliers")]
        [ConfigurationCollection(typeof(SupplierElement), AddItemName = "supplier")]
        public SuppliersElementCollection Suppliers
        {
            get
            {
                return ((SuppliersElementCollection)(this["suppliers"]));
            }
        }

        [ConfigurationProperty("color-mappings")]
        [ConfigurationCollection(typeof(ColorMappingElement), AddItemName = "add")]
        public ColorMappingElementCollection Colors
        {
            get
            {
                return ((ColorMappingElementCollection)(this["color-mappings"]));
            }
        }
    }
}

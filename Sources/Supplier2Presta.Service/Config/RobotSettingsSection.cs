using System.Configuration;

namespace Supplier2Presta.Service.Config
{
    public sealed partial class RobotSettingsSection : ConfigurationSection
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
    }
}

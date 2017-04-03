using System.Configuration;

namespace Supplier2Presta.Service.Config
{
	public sealed class RobotSettingsSection : ConfigurationSection
	{
		[ConfigurationProperty("suppliers")]
		[ConfigurationCollection(typeof(SupplierElement), AddItemName = "supplier")]
		public SuppliersElementCollection Suppliers => (SuppliersElementCollection)this["suppliers"];

		[ConfigurationProperty("color-mappings")]
		[ConfigurationCollection(typeof(ColorMappingElement), AddItemName = "add")]
		public ColorMappingElementCollection Colors => (ColorMappingElementCollection)this["color-mappings"];

		[ConfigurationProperty("ignored-products")]
		[ConfigurationCollection(typeof(IgnoredProductElement), AddItemName = "add")]
		public IgnoredProductElementCollection IgnoredProducts => (IgnoredProductElementCollection)this["ignored-products"];
	}
}

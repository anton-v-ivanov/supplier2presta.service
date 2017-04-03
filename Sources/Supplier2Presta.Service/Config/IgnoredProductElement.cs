using System.Configuration;

namespace Supplier2Presta.Service.Config
{
	public class IgnoredProductElement : ConfigurationElement
	{
		[ConfigurationProperty("reference", IsKey = true, IsRequired = true)]
		public string Reference => (string)this["reference"];
	}
}

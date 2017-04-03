using System.Configuration;

namespace Supplier2Presta.Service.Config
{
	public class IgnoredProductElementCollection : ConfigurationElementCollection
	{
		public IgnoredProductElement this[int i] => (IgnoredProductElement)BaseGet(i);

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((IgnoredProductElement)element).Reference;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new IgnoredProductElement();
		}
	}
}
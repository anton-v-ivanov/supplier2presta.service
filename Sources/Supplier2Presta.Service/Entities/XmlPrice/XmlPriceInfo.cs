using System;
using System.Xml.Serialization;

namespace Supplier2Presta.Service.Entities.XmlPrice
{
	[Serializable]
	public class XmlPriceInfo
	{
		[XmlAttribute("RetailPrice")]
		public string RetailPrice { get; set; }

		[XmlAttribute("WholePrice")]
		public string Wholesale { get; set; }
	}
}

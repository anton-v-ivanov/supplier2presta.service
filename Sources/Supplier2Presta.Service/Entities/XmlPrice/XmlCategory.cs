using System;
using System.Xml.Serialization;

namespace Supplier2Presta.Service.Entities.XmlPrice
{
	[Serializable]
	public class XmlCategory
	{
		/// <summary>
		/// Название основного раздела
		/// </summary>
		[XmlElement("catName")]
		public string Name { get; set; }

		/// <summary>
		/// Название подразделов
		/// </summary>
		[XmlElement("subName")]
		public string SubName { get; set; }
	}
}
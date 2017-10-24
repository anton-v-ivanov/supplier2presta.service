using System;
using System.Xml.Serialization;

namespace Supplier2Presta.Entities.XmlPrice
{
	[Serializable]
	public class XmlCategory
	{
        /// <summary>
        /// Название основного раздела
        /// </summary>
        [XmlAttribute("Name")]
        //[XmlElement("catName")]
		public string Name { get; set; }

		/// <summary>
		/// Название подразделов
		/// </summary>
		//[XmlElement("subName")]
        [XmlAttribute("subName")]
		public string SubName { get; set; }
	}
}
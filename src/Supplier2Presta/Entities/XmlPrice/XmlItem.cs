using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Supplier2Presta.Entities.XmlPrice
{
	[Serializable]
	public class XmlItem
	{
		/// <summary>
		/// Артикул товар
		/// </summary>
		[XmlAttribute("prodID")]
		public string Id { get; set; }

		/// <summary>
		/// Основная категория товара
		/// </summary>
		[XmlArray("categories")]
		[XmlArrayItem("category")]
		public List<XmlCategory> Categories { get; set; }

		/// <summary>
		/// Наименование товара
		/// </summary>
		[XmlAttribute("name")]
		public string Name { get; set; }

		/// <summary>
		/// Описание товара
		/// </summary>
		[XmlElement("description")]
		public string Description { get; set; }

		/// <summary>
		/// Производитель
		/// </summary>
		[XmlAttribute("vendor")]
		public string Vendor { get; set; }

		/// <summary>
		/// Артикул производителя
		/// </summary>
		[XmlAttribute("vendorCode")]
		public string VendorCode { get; set; }

		[XmlElement("price")]
		public XmlPriceInfo PriceInfo { get; set; }

		[XmlAttribute("pack")]
		public string Packing { get; set; }

		[XmlAttribute("material")]
		public string Material { get; set; }

		[XmlAttribute("lenght")]
		public string Length { get; set; }

		[XmlAttribute("diameter")]
		public string Diameter { get; set; }

		[XmlAttribute("brutto")]
		public string Weight { get; set; }

		[XmlAttribute("batteries")]
		public string Batteries { get; set; }

		/// <summary>
		/// url маленькой картинки до 150*150 пикселей
		/// </summary>
		[XmlElement("pictureSmall")]
		public string PictureSmall { get; set; }

		/// <summary>
		/// Адреса больших картинок до 1200*1200 пикселей
		/// </summary>
		[XmlArray("pictures")]
		[XmlArrayItem("picture")]
		public List<string> Pictures { get; set; }

		/// <summary>
		/// Данные по ассортименту
		/// </summary>
		[XmlArray("assortiment")]
		[XmlArrayItem("assort")]
		public List<XmlAssort> Assort { get; set; }
	}
}

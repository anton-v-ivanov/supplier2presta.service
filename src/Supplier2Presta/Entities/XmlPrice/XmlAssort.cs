using System;
using System.Xml.Serialization;

namespace Supplier2Presta.Entities.XmlPrice
{
	[Serializable]
	public class XmlAssort
	{
		/// <summary>
		/// Цвет
		/// </summary>
    [XmlAttribute("color")]
		public string Color { get; set; }
        
		/// <summary>
		/// Размер
		/// </summary>
    [XmlAttribute("size")]
		public string Size { get; set; }
        
		/// <summary>
		/// Количество незабронированных моделей на складе
		/// </summary>
		[XmlAttribute("sklad")]
		public int Sklad { get; set; }
        
		/// <summary>
		/// Скорость откгрузки
		/// </summary>
		[XmlAttribute("ShippingDate")]
		public string ShippingDate { get; set; }
        
		/// <summary>
		/// Уникальный идентификатор товарного предложения
		/// </summary>
		[XmlAttribute("aID")]
		public string Aid { get; set; }
        
		/// <summary>
		/// Штрихкод товарного предложения
		/// </summary>
		[XmlAttribute("barcode")]
		public string Barcode { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Supplier2Presta.Service.Entities.XmlPrice
{
    [Serializable]
    public class XmlItem
    {
        /// <summary>
        /// Артикул товар
        /// </summary>
        [XmlElement("id")]
        public string Id { get; set; }

        /// <summary>
        /// Основная категория товара
        /// </summary>
        [XmlElement("category")]
        public XmlCategory Category { get; set; }

        /// <summary>
        /// Наименование товара
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Описание товара
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// Производитель
        /// </summary>
        [XmlElement("vendor")]
        public string Vendor { get; set; }

        /// <summary>
        /// Артикул производителя
        /// </summary>
        [XmlElement("vendorCode")]
        public string VendorCode { get; set; }

        /// <summary>
        /// Рекомендованная розничная цена (рубли)
        /// </summary>
        [XmlElement("price")]
        public string Price { get; set; }

        /// <summary>
        /// Оптовая цена (рубли)
        /// </summary>
        [XmlElement("wholesale")]
        public string Wholesale { get; set; }

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
        [XmlElement("assort")]
        public List<XmlAssort> Assort { get; set; }
    }
    
    [Serializable]
    public class XmlCategory
    {
        /// <summary>
        /// Название основного раздела
        /// </summary>
        [XmlElement("catName")]
        public string CatName { get; set; }

        /// <summary>
        /// Название подразделов
        /// </summary>
        [XmlElement("subName")]
        public string SubName { get; set; }
    }

    [Serializable]
    public class XmlAssort
    {
        /// <summary>
        /// Цвет
        /// </summary>
        [XmlElement("color")]
        public string Color { get; set; }
        
        /// <summary>
        /// Размер
        /// </summary>
        [XmlElement("size")]
        public string Size { get; set; }
        
        /// <summary>
        /// Количество незабронированных моделей на складе
        /// </summary>
        [XmlElement("sklad")]
        public int Sklad { get; set; }
        
        /// <summary>
        /// Скорость откгрузки (1 - в течении часа с момента заказа, 2 - в течении одного рабочего дня, 3 - время поставки не известно)
        /// </summary>
        [XmlElement("time")]
        public string Time { get; set; }
        
        /// <summary>
        /// Уникальный идентификатор товарного предложения
        /// </summary>
        [XmlElement("aID")]
        public string Aid { get; set; }
        
        /// <summary>
        /// Штрихкод товарного предложения
        /// </summary>
        [XmlElement("barcode")]
        public string Barcode { get; set; }
    }
}

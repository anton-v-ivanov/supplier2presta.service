using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.PriceItemBuilders;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.PricefileItemBuilders
{
    public class XmlHappinnessPriceItemBuilder : IPriceItemBuilder<XmlItem>
    {
        public PriceItem Build(XmlItem fileItem)
        {
            var result = new PriceItem
            {
                Active = true,
                Balance = fileItem.Assort.Sklad,
                Color = fileItem.Assort.Color,
                Ean13 = fileItem.Assort.Barcode,
                Manufacturer = fileItem.Vendor,
                Name = !string.IsNullOrWhiteSpace(fileItem.Name) ? fileItem.Name.MakeSafeName() : string.Empty,
                Photo1 = fileItem.PictureSmall,
                Photo2 = fileItem.Pictures.Count > 0 ? fileItem.Pictures[0] : string.Empty,
                Photo3 = fileItem.Pictures.Count > 1 ? fileItem.Pictures[1] : string.Empty,
                Photo4 = fileItem.Pictures.Count > 2 ? fileItem.Pictures[2] : string.Empty,
                Photo5 = fileItem.Pictures.Count > 3 ? fileItem.Pictures[3] : string.Empty,
                Reference = "200" + fileItem.Id.Trim(new[] { '"', ';' }),
                RetailPrice = float.Parse(fileItem.Price.Trim(new[] { '"', ';' }).Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),
                Size = fileItem.Assort.Size,
                SupplierName = "happiness",
                SupplierReference = fileItem.VendorCode,
                WholesalePrice = float.Parse(fileItem.Wholesale.Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),
            };
            
            if(fileItem.Category != null)
            {
                result.Category = !string.IsNullOrWhiteSpace(fileItem.Category.SubName) ? fileItem.Category.SubName : fileItem.Category.CatName;
            }

            if(!string.IsNullOrEmpty(result.SupplierReference) && result.SupplierReference.Length > 32)
            {
                result.SupplierReference = result.SupplierReference.Substring(0, 32);
            }

            if (!string.IsNullOrEmpty(fileItem.Description))
            {
                string shortDescription;
                var parameters = Helper.ParseDescription(fileItem.Description.Trim(new[] { '"', ';' }), out shortDescription);

                result.ShortDescription = shortDescription;
                result.Description = string.Empty;
                if (shortDescription.Length > 800)
                {
                    result.Description = shortDescription;
                    result.ShortDescription = shortDescription.TruncateAtWord(796);
                }

                result.Length = parameters.ContainsKey("Длина") ? parameters["Длина"] : string.Empty;
                result.Diameter = parameters.ContainsKey("Диаметр") ? parameters["Диаметр"] : string.Empty;
            }

            return result;
        }
    }
}

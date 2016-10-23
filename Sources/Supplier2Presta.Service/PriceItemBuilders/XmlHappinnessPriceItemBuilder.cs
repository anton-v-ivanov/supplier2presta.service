using System.Globalization;
using System.Text.RegularExpressions;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Helpers;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public class XmlHappinnessPriceItemBuilder : IPriceItemBuilder<XmlItem>
    {
        private readonly Regex _ean13Regex = new Regex("[0-9]{12}", RegexOptions.Compiled);
        private readonly IColorBuilder _colorCodeBuilder;

        public XmlHappinnessPriceItemBuilder(IColorBuilder colorCodeBuilder)
        {
            _colorCodeBuilder = colorCodeBuilder;
        }

        public PriceItem Build(XmlItem fileItem)
        {
            var result = new PriceItem
            {
                Active = true,
                Manufacturer = fileItem.Vendor,
                Name = !string.IsNullOrWhiteSpace(fileItem.Name) ? fileItem.Name.MakeSafeName() : string.Empty,
                PhotoSmall = fileItem.PictureSmall,
                Reference = "200" + fileItem.Id.Trim('"', ';'),
                RetailPrice = float.Parse(fileItem.Price.Trim('"', ';').Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),
                
                SupplierName = "happiness",
                SupplierReference = fileItem.VendorCode,
                WholesalePrice = float.Parse(fileItem.Wholesale.Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),
            };

            if(fileItem.Pictures != null)
            {
                foreach (var picture in fileItem.Pictures)
	            {
                    result.Photos.Add(picture);
	            }
            }

            if(fileItem.Assort != null)
            {
                foreach (var assort in fileItem.Assort)
	            {
                    var a = new Assort
                        {
                            Balance = assort.Sklad,
                            Color = !string.IsNullOrWhiteSpace(assort.Color) ? _colorCodeBuilder.GetMainColor(assort.Color.FirstLetterToUpper()) : string.Empty,
                            Size = assort.Size,
                            Ean13 = assort.Barcode,
                            Reference = "200" + assort.Aid,
                        };
                    a.ColorCode = !string.IsNullOrWhiteSpace(a.Color) ? _colorCodeBuilder.GetCode(a.Color) : string.Empty;
                    if (!string.IsNullOrWhiteSpace(a.Ean13))
                    {
                        a.Ean13 = _ean13Regex.Match(a.Ean13).Value;
                    }

                    result.Assort.Add(a);
	            }
            }

            if (!string.IsNullOrWhiteSpace(result.Ean13))
            {
                result.Ean13 = _ean13Regex.Match(result.Ean13).Value;
            }

            if(fileItem.Category != null)
            {
                result.RootCategory = fileItem.Category.CatName;
                result.Category = !string.IsNullOrWhiteSpace(fileItem.Category.SubName) ? fileItem.Category.SubName : fileItem.Category.CatName;
            }

            if(!string.IsNullOrEmpty(result.SupplierReference) && result.SupplierReference.Length > 32)
            {
                result.SupplierReference = result.SupplierReference.Substring(0, 32);
            }

            if (!string.IsNullOrEmpty(fileItem.Description))
            {
                string shortDescription;
                var parameters = Helper.ParseDescription(fileItem.Description.Trim('"', ';'), out shortDescription);

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

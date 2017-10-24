using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Supplier2Presta.Entities;
using Supplier2Presta.Entities.XmlPrice;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.PriceItemBuilders
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
				RetailPrice = float.Parse(fileItem.PriceInfo.RetailPrice.Trim('"', ';').Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),

				SupplierName = "happiness",
				SupplierReference = fileItem.VendorCode,
				WholesalePrice = float.Parse(fileItem.PriceInfo.Wholesale.Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." }),
			};
			if (!string.IsNullOrWhiteSpace(fileItem.Packing))
				result.Packing = fileItem.Packing.FirstLetterToUpper();
			if (!string.IsNullOrWhiteSpace(fileItem.Length))
				result.Length = fileItem.Length;
			if (!string.IsNullOrWhiteSpace(fileItem.Weight))
				result.Weight = fileItem.Weight;
			if (!string.IsNullOrWhiteSpace(fileItem.Material))
				result.Material = fileItem.Material.FirstLetterToUpper();
			if (!string.IsNullOrWhiteSpace(fileItem.Batteries))
				result.Battery = fileItem.Batteries.FirstLetterToUpper().CapitalizeEnglish();
			if (!string.IsNullOrWhiteSpace(fileItem.Diameter))
				result.Diameter = fileItem.Diameter;

			if (fileItem.Pictures != null)
			{
				foreach (var picture in fileItem.Pictures)
				{
					result.Photos.Add(picture);
				}
			}

			if (fileItem.Assort != null)
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
					if (!string.IsNullOrEmpty(a.Ean13))
					{
						a.Ean13 = _ean13Regex.Match(a.Ean13).Value.Replace(" ", "");
					}
					result.Assort.Add(a);
				}
			}

			if (!string.IsNullOrWhiteSpace(result.Ean13))
			{
				result.Ean13 = _ean13Regex.Match(result.Ean13).Value;
			}

			if (fileItem.Categories != null)
			{
				result.Categories = new List<CategoryInfo>();
			    foreach (var category in fileItem.Categories)
			    {
                    if (string.IsNullOrEmpty(category.Name))
                        throw new Exception("Category is null");

                    result.Categories.Add(new CategoryInfo(category.Name, !string.IsNullOrWhiteSpace(category.SubName) ? category.SubName : category.Name));
			    }
			}

			if (!string.IsNullOrEmpty(result.SupplierReference) && result.SupplierReference.Length > 32)
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

				if (string.IsNullOrEmpty(result.Length))
					result.Length = parameters.ContainsKey("Длина") ? parameters["Длина"] : string.Empty;

				if (string.IsNullOrEmpty(result.Diameter))
					result.Diameter = parameters.ContainsKey("Диаметр") ? parameters["Диаметр"] : string.Empty;
			}

			return result;
		}
	}
}

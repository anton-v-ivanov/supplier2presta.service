using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.PriceItemBuilders
{
    public class CsvHappinnessPriceItemBuilder : IPriceItemBuilder<string>
    {
        //private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(""?[\w\s./\:\-\?,–]*""?;)|([\w\s./:-]*$)", RegexOptions.Compiled);
		//private static readonly Regex SizeRegex = new Regex(@"[A-Za-z]+|безразмерный", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//private static readonly Regex ColorRegex = new Regex(@"[А-Яа-я ]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _ean13Regex = new Regex("[0-9]{12}", RegexOptions.Compiled);
        
        private readonly PriceFormat _priceFormat;

        public CsvHappinnessPriceItemBuilder(PriceFormat priceFormat)
        {
            _priceFormat = priceFormat;
        }

        public PriceItem Build(string line)
        {
            var columns = LineRegex.Matches(line);

            var result = new PriceItem
            {
                SupplierReference = _priceFormat.SupplierReference > -1 ? columns[_priceFormat.SupplierReference].Value.Trim('"', ';') : string.Empty,
                WholesalePrice = float.Parse(
                    columns[_priceFormat.WholesalePrice].Value.Trim('"', ';')
                    .Replace(" ", "")
                    .Replace(",", "."),
                    new NumberFormatInfo { NumberDecimalSeparator = "." }),
                Manufacturer = _priceFormat.Manufacturer > -1 ? columns[_priceFormat.Manufacturer].Value.Trim('"', ';') : string.Empty,
                PhotoSmall = _priceFormat.Photo1 > -1 ? columns[_priceFormat.Photo1].Value.Trim('"', ';') : string.Empty,
            };

            if (result.SupplierReference.Length > 32)
            {
                result.SupplierReference = result.SupplierReference.Substring(0, 32);
            }

            if (_priceFormat.Ean13 > -1 && columns.Count > _priceFormat.Ean13)
            {
                result.Ean13 = _ean13Regex.Match(columns[_priceFormat.Ean13].Value.Trim('"', ';').Trim()).Value;
                if(result.Ean13.Length > 13)
                {
                    result.Ean13 = result.Ean13.Substring(0, 13);
                }
            }

            if (_priceFormat.Description > -1)
            {
                string shortDescription;
                var parameters = Helper.ParseDescription(columns[_priceFormat.Description].Value.Trim('"', ';'), out shortDescription);

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

            result.RetailPrice = float.Parse(columns[_priceFormat.RetailPrice].Value.Trim('"', ';').Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." });

            result.Assort.Add(
                new Assort
                {
                    Balance = Convert.ToInt32(columns[_priceFormat.Balance].Value.Trim('"', ';')),
                    Size = _priceFormat.Size > -1 ? columns[_priceFormat.Size].Value.Trim('"', ';') : string.Empty,
                    Color = _priceFormat.Color > -1 ? columns[_priceFormat.Color].Value.Trim('"', ';').FirstLetterToUpper() : string.Empty,
                    Reference = _priceFormat.Aid > -1 ? "200" + columns[_priceFormat.Aid].Value.Trim('"', ';') : null,
                });

            result.Material = _priceFormat.Material > -1 ? columns[_priceFormat.Material].Value.Trim('"', ';').FirstLetterToUpper() : string.Empty;
            result.Country = _priceFormat.Country > -1 ? columns[_priceFormat.Country].Value.Trim('"', ';').FirstLetterToUpper() : string.Empty;
            result.Packing = _priceFormat.Packing > -1 ? columns[_priceFormat.Packing].Value.Trim('"', ';').FirstLetterToUpper() : string.Empty;

            result.Name = _priceFormat.Name > -1 ? columns[_priceFormat.Name].Value.Trim('"', ';').MakeSafeName() : string.Empty;
            result.SupplierName = "happiness";
            result.Reference = "200" + columns[_priceFormat.Reference].Value.Trim('"', ';');

			var categories = GetCategoryName(columns);
			result.Categories.Add(new CategoryInfo(categories.Item1, categories.Item2));

            result.Active = _priceFormat.Active <= -1 || Convert.ToBoolean(Convert.ToInt32(columns[_priceFormat.Active].Value.Trim('"', ';')));
            result.Battery = _priceFormat.Battery > -1 ? columns[_priceFormat.Battery].Value.Trim('"', ';', ' ').FirstLetterToUpper().CapitalizeEnglish() : string.Empty;
            result.Weight = _priceFormat.Weight > -1 ? columns[_priceFormat.Weight].Value.Trim('"', ';') : string.Empty;

            if (_priceFormat.Photo2 > -1)
                result.Photos.Add(columns[_priceFormat.Photo2].Value.Trim('"', ';'));
            if(_priceFormat.Photo3 > -1)
                result.Photos.Add(columns[_priceFormat.Photo3].Value.Trim('"', ';'));
            if(_priceFormat.Photo4 > -1)
                result.Photos.Add(columns[_priceFormat.Photo4].Value.Trim('"', ';'));
            if(_priceFormat.Photo5 > -1)
                result.Photos.Add(columns[_priceFormat.Photo5].Value.Trim('"', ';'));
            
            //if(PriceFormat.Color < 0 && PriceFormat.Size > -1)
            //{
            //    result = ParseSizeValue(result);
            //}
            
            return result;
        }

        /// <summary>
        /// Return root and main category
        /// </summary>
        /// <param name="columns"></param>
        /// <returns>Returns Tuple<rootCategory, mainCategory></returns>
        private Tuple<string, string> GetCategoryName(MatchCollection columns)
        {
            if (_priceFormat.Category1 < 0)
                return Tuple.Create(string.Empty, string.Empty);

            var parent = columns[_priceFormat.Category1].Value.Trim('"', ';').FirstLetterToUpper();
            if (_priceFormat.Category2 > -1)
            {
                var child1 = columns[_priceFormat.Category2].Value.Trim('"', ';').FirstLetterToUpper();
                string child2 = null;
                if (_priceFormat.Category3 > -1)
                {
                    child2 = columns[_priceFormat.Category3].Value.Trim('"', ';').FirstLetterToUpper();
                }

                if (!string.IsNullOrWhiteSpace(child2))
                {
                    return Tuple.Create(child1, child2);
                }

                if (!string.IsNullOrWhiteSpace(child1))
                {
                    return Tuple.Create(parent, child1);
                }
            }
            if (!string.IsNullOrWhiteSpace(parent))
            {
                return Tuple.Create(parent, parent);
            }

            return Tuple.Create(string.Empty, string.Empty);
        }
    }
}

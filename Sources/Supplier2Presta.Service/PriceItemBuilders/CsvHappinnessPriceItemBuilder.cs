using System;
using System.Text.RegularExpressions;

using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using System.Collections.Generic;
using System.Globalization;
using Supplier2Presta.Service.Multiplicators;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public class CsvHappinnessPriceItemBuilder : IPriceItemBuilder<string>
    {
        //private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(""?[\w\s./\:\-\?,–]*""?;)|([\w\s./:-]*$)", RegexOptions.Compiled);
		//private static readonly Regex SizeRegex = new Regex(@"[A-Za-z]+|безразмерный", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		//private static readonly Regex ColorRegex = new Regex(@"[А-Яа-я ]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private readonly PriceFormat PriceFormat;

        public CsvHappinnessPriceItemBuilder(PriceFormat priceFormat)
        {
            this.PriceFormat = priceFormat;
        }

        public PriceItem Build(string line)
        {
            var columns = LineRegex.Matches(line);

            var result = new PriceItem
            {
                SupplierReference = this.PriceFormat.SupplierReference > -1 ? columns[this.PriceFormat.SupplierReference].Value.Trim(new[] { '"', ';' }) : string.Empty,
                WholesalePrice = float.Parse(
                    columns[this.PriceFormat.WholesalePrice].Value.Trim(new[] { '"', ';' })
                    .Replace(" ", "")
                    .Replace(",", "."),
                    new NumberFormatInfo { NumberDecimalSeparator = "." }),
                Manufacturer = this.PriceFormat.Manufacturer > -1 ? columns[this.PriceFormat.Manufacturer].Value.Trim(new[] { '"', ';' }) : string.Empty,
                Photo1 = this.PriceFormat.Photo1 > -1 ? columns[this.PriceFormat.Photo1].Value.Trim(new[] { '"', ';' }) : string.Empty,
                Balance = Convert.ToInt32(columns[this.PriceFormat.Balance].Value.Trim(new[] { '"', ';' })),
            };

            if (result.SupplierReference.Length > 32)
            {
                result.SupplierReference = result.SupplierReference.Substring(0, 32);
            }

            if (this.PriceFormat.Ean13 > -1 && columns.Count > this.PriceFormat.Ean13)
            {
                result.Ean13 = columns[this.PriceFormat.Ean13].Value.Trim(new[] { '"', ';' }).Trim();
            }

            if (this.PriceFormat.Description > -1)
            {
                string shortDescription;
                var parameters = Helper.ParseDescription(columns[this.PriceFormat.Description].Value.Trim(new[] { '"', ';' }), out shortDescription);

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

            result.RetailPrice = float.Parse(columns[this.PriceFormat.RetailPrice].Value.Trim(new[] { '"', ';' }).Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." });

            result.Size = this.PriceFormat.Size > -1 ? columns[this.PriceFormat.Size].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Color = this.PriceFormat.Color > -1 ? columns[this.PriceFormat.Color].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper() : string.Empty;
            result.Material = this.PriceFormat.Material > -1 ? columns[this.PriceFormat.Material].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper() : string.Empty;
            result.Country = this.PriceFormat.Country > -1 ? columns[this.PriceFormat.Country].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper() : string.Empty;
            result.Packing = this.PriceFormat.Packing > -1 ? columns[this.PriceFormat.Packing].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper() : string.Empty;

            result.Name = this.PriceFormat.Name > -1 ? columns[this.PriceFormat.Name].Value.Trim(new[] { '"', ';' }).MakeSafeName() : string.Empty;
            result.SupplierName = "happiness";
            result.Reference = "200" + columns[this.PriceFormat.Reference].Value.Trim(new[] { '"', ';' });
            var categories = this.GetCategoryName(columns);
            result.RootCategory = categories.Item1;
            result.Category = categories.Item2;
            result.Active = this.PriceFormat.Active > -1 ? Convert.ToBoolean(Convert.ToInt32(columns[this.PriceFormat.Active].Value.Trim(new[] { '"', ';' }))) : true;
            result.Battery = this.PriceFormat.Battery > -1 ? columns[this.PriceFormat.Battery].Value.Trim(new[] { '"', ';', ' ' }).FirstLetterToUpper().CapitalizeEnglish() : string.Empty;
            result.Photo2 = this.PriceFormat.Photo2 > -1 ? columns[this.PriceFormat.Photo2].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo3 = this.PriceFormat.Photo3 > -1 ? columns[this.PriceFormat.Photo3].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo4 = this.PriceFormat.Photo4 > -1 ? columns[this.PriceFormat.Photo4].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo5 = this.PriceFormat.Photo5 > -1 ? columns[this.PriceFormat.Photo5].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Weight = this.PriceFormat.Weight > -1 ? columns[this.PriceFormat.Weight].Value.Trim(new[] { '"', ';' }) : string.Empty;
            
            //if(this.PriceFormat.Color < 0 && this.PriceFormat.Size > -1)
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
            if (this.PriceFormat.Category1 < 0)
                return Tuple.Create<string, string>(string.Empty, string.Empty);

            var parent = columns[this.PriceFormat.Category1].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            if (this.PriceFormat.Category2 > -1)
            {
                var child1 = columns[this.PriceFormat.Category2].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                string child2 = null;
                if (this.PriceFormat.Category3 > -1)
                {
                    child2 = columns[this.PriceFormat.Category3].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                }

                if (!string.IsNullOrWhiteSpace(child2))
                {
                    return Tuple.Create<string, string>(child1, child2);
                }

                if (!string.IsNullOrWhiteSpace(child1))
                {
                    return Tuple.Create<string, string>(parent, child1);
                }
            }
            if (!string.IsNullOrWhiteSpace(parent))
            {
                return Tuple.Create<string, string>(parent, parent);
            }

            return Tuple.Create<string, string>(string.Empty, string.Empty);
        }
    }
}

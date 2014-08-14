using System;
using System.Globalization;
using System.Text.RegularExpressions;

using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.PriceItemBuilders
{
    public class PriceItemBuilderBase
    {
        protected readonly PriceFormat PriceFormat;
        private readonly float? multiplicator;
        private readonly bool generateProperties;

        public PriceItemBuilderBase(PriceFormat priceFormat, float? multiplicator, bool generateProperties)
        {
            this.PriceFormat = priceFormat;
            this.multiplicator = multiplicator;
            this.generateProperties = generateProperties;
        }

        protected PriceItem Build(MatchCollection columns)
        {
            var result = new PriceItem
            {
                SupplierReference = columns[this.PriceFormat.SupplierReference].Value.Trim(new[] { '"', ';' }),
                WholesalePrice = float.Parse(
                    columns[this.PriceFormat.WholesalePrice].Value.Trim(new[] { '"', ';' })
                    .Replace(" ", "")
                    .Replace(",", "."),
                    new NumberFormatInfo { NumberDecimalSeparator = "." }),
                Manufacturer = columns[this.PriceFormat.Manufacturer].Value.Trim(new[] { '"', ';' }),
                Photo1 = columns[this.PriceFormat.Photo1].Value.Trim(new[] { '"', ';' }),
                Balance = Convert.ToInt32(columns[this.PriceFormat.Balance].Value.Trim(new[] { '"', ';' })),
            };

            if(result.SupplierReference.Length > 32)
            {
                result.SupplierReference = result.SupplierReference.Substring(0, 32);
            }

            if (columns.Count > this.PriceFormat.Ean13)
            {
                result.Ean13 = columns[this.PriceFormat.Ean13].Value.Trim(new[] { '"', ';' }).Trim();
            }

            string shortDescription;
            var parameters = Helper.ParseDescription(columns[this.PriceFormat.Description].Value.Trim(new[] { '"', ';' }), out shortDescription);

            result.ShortDescription = shortDescription;
            result.Description = string.Empty;
            if (shortDescription.Length > 800)
            {
                result.Description = shortDescription;
                result.ShortDescription = shortDescription.TruncateAtWord(796);
            }

            result.RetailPrice = !multiplicator.HasValue
                ? float.Parse(columns[this.PriceFormat.RetailPrice].Value.Trim(new[] { '"', ';' }).Replace(" ", "").Replace(",", "."), new NumberFormatInfo { NumberDecimalSeparator = "." })
                : Convert.ToInt32(Math.Ceiling(result.WholesalePrice * multiplicator.Value));

            if (generateProperties)
            {
                result.Size = Helper.GenerateProperty("Размер", columns[this.PriceFormat.Size], parameters);
                result.Color = Helper.GenerateProperty("Цвет", columns[this.PriceFormat.Color], parameters);
                result.Material = Helper.GenerateProperty("Материал", columns[this.PriceFormat.Material], parameters);
                if (this.PriceFormat.Country >= 0)
                {
                    result.Country = Helper.GenerateProperty("Страна", columns[this.PriceFormat.Country], parameters);
                }
                result.Packing = Helper.GenerateProperty("Упаковка", columns[this.PriceFormat.Packing], parameters);
                result.Length = Helper.GenerateProperty("Длина", Match.Empty, parameters);
                result.Diameter = Helper.GenerateProperty("Диаметр", Match.Empty, parameters);
            }
            else
            {
                result.Size = columns[this.PriceFormat.Size].Value.Trim(new[] { '"', ';' });
                result.Color = columns[this.PriceFormat.Color].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                result.Material = columns[this.PriceFormat.Material].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                if (this.PriceFormat.Country >= 0)
                {
                    result.Country = columns[this.PriceFormat.Country].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                }

                result.Packing = columns[this.PriceFormat.Packing].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                result.Length = parameters.ContainsKey("Длина") ? parameters["Длина"] : string.Empty;
                result.Diameter = parameters.ContainsKey("Диаметр") ? parameters["Диаметр"] : string.Empty;
            }

            return result;
        }
    }
}

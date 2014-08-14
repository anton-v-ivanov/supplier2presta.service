using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.PriceItemBuilders
{
    public class SexsnabPriceItemBuilder : PriceItemBuilderBase, IPriceItemBuilder
    {
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);

        private readonly Dictionary<string, string> shortenings;

        public SexsnabPriceItemBuilder(PriceFormat priceFormat, float? multiplicator, Dictionary<string, string> shortenings, bool generateProperties)
            : base(priceFormat, multiplicator, generateProperties)
        {
            this.shortenings = shortenings;
        }

        public PriceItem Build(string line)
        {
            var columns = LineRegex.Matches(line);

            if (columns.Count != 21)
            {
                throw new Exception(string.Format("Неправильный формат строки: {0}", line));
            }

            var result = this.Build(columns);
            result.Name = Helper.ReplaceShortenings(Helper.NormalizeName(columns[this.PriceFormat.Name].Value.Trim(new[] { '"', ';' })), this.shortenings).MakeSafeName();
            result.SupplierName = "sexsnab";
            result.Reference = "100" + columns[this.PriceFormat.Reference].Value.Trim(new[] { '"', ';' });
            result.Category = GetCategoryName(columns);
            result.Active = true;

            return result;
        }

        private string GetCategoryName(MatchCollection columns)
        {
            var parent = columns[PriceFormat.Category1].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            var child1 = columns[PriceFormat.Category2].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            var child2 = columns[PriceFormat.Category3].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();

            if (!string.IsNullOrWhiteSpace(child2))
            {
                return child2;
            }

            if (!string.IsNullOrWhiteSpace(child1))
            {
                return child1;
            }

            if (!string.IsNullOrWhiteSpace(parent))
            {
                return parent;
            }

            return string.Empty;
        }
    }
}
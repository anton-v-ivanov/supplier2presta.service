using System;
using System.Text.RegularExpressions;

using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public class HappinessPriceItemBuilder : PriceItemBuilderBase, IPriceItemBuilder
    {
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);
        public HappinessPriceItemBuilder(PriceFormat priceFormat, float? multiplicator)
            : base(priceFormat, multiplicator)
        {
        }

        public PriceItem Build(string line)
        {
            var columns = LineRegex.Matches(line);
            
            var result = this.Build(columns);
            result.Name = this.PriceFormat.Name > -1 ? columns[this.PriceFormat.Name].Value.Trim(new[] { '"', ';' }).MakeSafeName() : string.Empty;
            result.SupplierName = "happiness";
            result.Reference = "200" + columns[this.PriceFormat.Reference].Value.Trim(new[] { '"', ';' });
            result.Category = this.GetCategoryName(columns);
            result.Active = this.PriceFormat.Active > -1 ? Convert.ToBoolean(Convert.ToInt32(columns[this.PriceFormat.Active].Value.Trim(new[] { '"', ';' }))) : true;
            result.Battery = this.PriceFormat.Battery > -1 ? columns[this.PriceFormat.Battery].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo2 = this.PriceFormat.Photo2 > -1 ? columns[this.PriceFormat.Photo2].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo3 = this.PriceFormat.Photo3 > -1 ? columns[this.PriceFormat.Photo3].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo4 = this.PriceFormat.Photo4 > -1 ? columns[this.PriceFormat.Photo4].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Photo5 = this.PriceFormat.Photo5 > -1 ? columns[this.PriceFormat.Photo5].Value.Trim(new[] { '"', ';' }) : string.Empty;
            result.Weight = this.PriceFormat.Weight > -1 ? columns[this.PriceFormat.Weight].Value.Trim(new[] { '"', ';' }) : string.Empty;

            return result;
        }

        private string GetCategoryName(MatchCollection columns)
        {
            if (this.PriceFormat.Category1 < 0)
                return string.Empty;

            var parent = columns[this.PriceFormat.Category1].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            var child1 = columns[this.PriceFormat.Category2].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            string child2 = null;
            if (this.PriceFormat.Category3 >= 0)
            {
                child2 = columns[this.PriceFormat.Category3].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
            }

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

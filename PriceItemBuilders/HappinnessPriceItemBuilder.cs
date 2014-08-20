using System;
using System.Text.RegularExpressions;

using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;

namespace Supplier2Presta.Service.PriceItemBuilders
{
    public class HappinessPriceItemBuilder : PriceItemBuilderBase, IPriceItemBuilder
    {
        //private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);
        private static readonly Regex LineRegex = new Regex(@"(""?[\w\s./:-]*""?;)|([\w\s./:-]*$)", RegexOptions.Compiled);
        private static readonly Regex SizeRegex = new Regex(@"([A-Za-z])|([А-Яа-я-]+)", RegexOptions.Compiled);
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
            
            if(this.PriceFormat.Color < 0 && this.PriceFormat.Size > -1)
            {
                result = ParseSizeValue(result);
            }
            
            return result;
        }

        private PriceItem ParseSizeValue(PriceItem result)
        {
            if(!string.IsNullOrWhiteSpace(result.Size))
            {
                var values = SizeRegex.Matches(result.Size);
                if(values.Count > 0)
                {
                    var size = string.Empty;
                    var color = string.Empty;
                    foreach (Match item in values)
                    {
                        if (item.Groups[1].Success)
                        {
                            if(!string.IsNullOrWhiteSpace(size))
                            {
                                size += ", ";
                            }

                            size += item.Groups[1].Value.Trim(new[] { '"', ';', '-' });
                        }
                        if (item.Groups[2].Success)
                        {
                            if (item.Groups[2].Value.Equals("безразмерный", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(size))
                                {
                                    size += ", ";
                                }
                                size += "Безразмерный";
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(color))
                                {
                                    color += ", ";
                                }

                                color += item.Groups[2].Value.Trim(new[] { '"', ';', '-' }).FirstLetterToUpper();
                            }
                        }
                    }
                    result.Size = size;
                    result.Color = color;
                }
            }
            return result;
        }

        private string GetCategoryName(MatchCollection columns)
        {
            if (this.PriceFormat.Category1 < 0)
                return string.Empty;

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
                    return child2;
                }

                if (!string.IsNullOrWhiteSpace(child1))
                {
                    return child1;
                }
            }
            if (!string.IsNullOrWhiteSpace(parent))
            {
                return parent;
            }

            return string.Empty;
        }
    }
}

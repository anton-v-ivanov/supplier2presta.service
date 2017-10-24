using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Supplier2Presta.Helpers
{
    internal class Helper
    {
        private static readonly Regex Reference = new Regex(@"/\s*", RegexOptions.Compiled);
        
        private static readonly Regex Parameters = new Regex(@"(Материал:\s?([A-Za-z\s]+\s?[A-Za-z\s]*))|(Цвет[:\s-–]+(\w+-?\w+))|([Дд]лина[:\s-–]+(\d+,?.?\s?[\d+]?\s?[смСМммММ]+))|([Дд]иаметр[:\s-–]+(\d+,?.?\s?[\d+]?\s?[смСМммММ]+))|([Уу]паковка[:\s-–]+([\w\s]+))", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex("[\\w`\"](\\s+([.,!;:?]))", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex2 = new Regex(@"([.,!;:?])(\w)", RegexOptions.Compiled);
        private static readonly Regex MultiSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);
        private static readonly Regex IntegerNormilizeRegex = new Regex(@"(\d+\s*[,.]?\d?)\s*([смСМммММ]+)", RegexOptions.Compiled);
        
        internal static string RemoveSupplierReference(string name)
        {
            var matches = Reference.Matches(name);
            if (matches.Count > 0)
            {
                var index = matches[0].Index;
                var clearName = name.Substring(index + matches[0].Length).Trim(new[] { '"', ';', ' ' });
                return clearName;
            }

            return name;
        }

        internal static Dictionary<string, string> ParseDescription(string desc, out string description)
        {
            var result = new Dictionary<string, string>();

            desc = RemoveSupplierReference(desc);
            var matches = Parameters.Matches(desc);
            foreach(Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    var material = match.Groups[2].Value;
                    result["Материал"] = material;
                    continue;
                }
                
                if (!string.IsNullOrWhiteSpace(match.Groups[4].Value))
                {
                    var color = match.Groups[4].Value;
                    if (color.Equals("в", StringComparison.OrdinalIgnoreCase))
                    {
                        color = "В ассортименте";
                    }

                    result["Цвет"] = color;
                    continue;
                }
                
                if (!string.IsNullOrWhiteSpace(match.Groups[6].Value))
                {
                    var intMatches = IntegerNormilizeRegex.Match(match.Groups[6].Value);
                    result["Длина"] = intMatches.Groups[1].Value.Replace(" ", "") + " " + intMatches.Groups[2].Value.Replace(" ", "");
                    continue;
                }
                
                if (!string.IsNullOrWhiteSpace(match.Groups[8].Value))
                {
                    var intMatches = IntegerNormilizeRegex.Match(match.Groups[8].Value);
                    result["Диаметр"] = intMatches.Groups[1].Value.Replace(" ", "") + " " + intMatches.Groups[2].Value.Replace(" ", "");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(match.Groups[10].Value))
                {
                    var pack = match.Groups[10].Value;
                    if (pack.Contains("коробка"))
                    {
                        pack = "Коробка";
                    }

                    result["Упаковка"] = pack;
                }
            }

            description = desc.Trim().Replace("`", "\"");
            var punctuationMatches = PunctuationRegex.Matches(description);
            description = punctuationMatches
                .Cast<Match>()
                .Aggregate(description, (current, match) => current.Replace(match.Groups[1].Value, match.Groups[2].Value));

            var punctuationMatches2 = PunctuationRegex2.Matches(description);
            description = punctuationMatches2
                .Cast<Match>()
                .Aggregate(description, (current, match) => current.Insert(match.Groups[2].Index, " "));

            var multispacesMatches = MultiSpacesRegex.Matches(description);
            description = multispacesMatches
                .Cast<Match>()
                .Aggregate(description, (current, match) => current.Replace(match.Value, " "));

            return result;
        }
    }
}

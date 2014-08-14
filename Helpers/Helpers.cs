using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Supplier2Presta.Entities;

namespace Supplier2Presta.Helpers
{
    internal class Helper
    {
        private static readonly Regex Reference = new Regex(@"/\s*", RegexOptions.Compiled);
        //private static readonly Regex Parameters = new Regex(@"([Мм]атериал:\s?(\w+))|(Цвет[:\s-–]+(\w+-?\w+))|([Дд]лина[:\s-–]+(\d+,?.?\s?[\d+]?\s?\w+))|([Дд]иаметр[:\s-–]+(\d+,?.?\s?[\d+]?\s?\w+))|([Уу]паковка[:\s-–]+(\w+))", RegexOptions.Compiled);
        
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

        internal static string NormalizeName(string name)
        {
            return RemoveSupplierReference(name).Replace(";", "").Replace("#", "№").FirstLetterToUpper();
        }

        internal static string ReplaceShortenings(string name, Dictionary<string, string> shortenings)
        {
            return shortenings.Aggregate(name, (current, shortening) => current.Replace(shortening.Key, shortening.Value));
        }

        internal static string GenerateProperty(string name, Match value, Dictionary<string, string> parameters)
        {
            // (Наименование:Значение:Позиция:Настроено)
            var propValue = value.Value.Trim(new[] { '"', ';' }).FirstLetterToUpper().CapitalizeEnglish();
            if (string.IsNullOrWhiteSpace(propValue))
            {
                if(parameters.ContainsKey(name))
                {
                    return string.Format("{0}:{1}:0:0", name, parameters[name].FirstLetterToUpper());
                }

                return string.Empty;
            }

            return string.Format("{0}:{1}:0:1", name, propValue);
        }

        internal static Dictionary<string, string> GetShortenings(IEnumerable<string> lines)
        {
            return lines
                .Select(line => line.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries))
                .Where(splits => splits.Count() > 1)
                .ToDictionary(splits => splits[0], splits => splits[1]);
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
                    if (color.Equals("в", StringComparison.InvariantCultureIgnoreCase))
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

        internal static string GetFileName(GeneratedPriceType generatedPriceType)
        {
            switch (generatedPriceType)
            {
                case GeneratedPriceType.NewItems:
                    return "\\new_products_{0}.csv";

                case GeneratedPriceType.DeletedItems:
                    return "\\deleted_products_{0}.csv";

                case GeneratedPriceType.SameItems:
                    return "\\same_products_{0}.csv";

                default:
                    throw new ArgumentOutOfRangeException("generatedPriceType");
            }
        }
    }
}

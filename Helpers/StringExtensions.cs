using System.Linq;
using System.Text.RegularExpressions;

namespace Supplier2Presta.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex EnglishLetters = new Regex(@"[a-z]", RegexOptions.Compiled);

        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
            {
                return input;
            }

            int nextSpace = input.LastIndexOf(" ", length, System.StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (nextSpace > 0) ? nextSpace : length).Trim());
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                if (str.Length < 5 && IsAllUpper(str))
                {
                    return str;
                }

                return char.ToUpper(str[0]) + str.Substring(1).ToLower();
            }

            return str.ToUpper();
        }

        public static string CapitalizeEnglish(this string str)
        {
            var englishLettersMatches = EnglishLetters.Matches(str);
            return englishLettersMatches
                .Cast<Match>()
                .Aggregate(str, (current, match) => current.Replace(match.Value, match.Value.ToUpperInvariant()));
        }

        private static bool IsAllUpper(string input)
        {
            return input.All(t => !char.IsLetter(t) || char.IsUpper(t));
        }

        public static string MakeSafeName(this string str)
        {
            //^<>;=#{}
            return str
                .Replace("^", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace(";", " ")
                .Replace("=", " ")
                .Replace("#", "№")
                .Replace("{", " ")
                .Replace("}", " ");
        }
    }
}
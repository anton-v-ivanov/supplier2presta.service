using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.CategoryBuilders
{
    public class CategoryFileBuilder : ICategoryBuilder
    {
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);

        private readonly PriceFormat priceFormat;

        private readonly string filePath;

        public CategoryFileBuilder(PriceFormat priceFormat, string filePath)
        {
            this.priceFormat = priceFormat;
            this.filePath = filePath;
        }

        public int Build(IEnumerable<string> lines)
        {
            int totalCount = 0;
            var sb = new StringBuilder();
            sb.AppendLine("Name;Parent;Photo;");

            var parents = new Dictionary<string, string>();
            var children1 = new Dictionary<string, Dictionary<string, string>>();
            var children2 = new Dictionary<string, Dictionary<string, string>>();

            var localLines = lines.Skip(1); // пропуск строки с заголовками

            // имя;артикул;код_1с;цена_оптовая;рубрика_меню_(родитель);рубрика_меню_(потомок_1);рубрика_меню_(потомок_2);производитель;описание;розничная_цена;фото;photo;section;остатки;размер;цвет;материал;страна;упаковка;штрихкод;тип_штрихкода
            foreach (var line in localLines)
            {
                var columns = LineRegex.Matches(line);

                var name = columns[priceFormat.Category1].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var photo = columns[priceFormat.Thumbnail].Value.Trim(new[] { '"', ';' });

                if (!parents.ContainsKey(name))
                {
                    parents.Add(name, photo);
                }

                if (priceFormat.Category2 > -1)
                {
                    var child1 = columns[priceFormat.Category2].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                    if (!string.IsNullOrWhiteSpace(child1))
                    {
                        if (!children1.ContainsKey(name))
                        {
                            children1.Add(name, new Dictionary<string, string>());
                        }

                        if (!children1[name].ContainsKey(child1))
                        {
                            children1[name].Add(child1, photo);
                        }
                    }

                    if (priceFormat.Category3 > -1)
                    {
                        var child2 = columns[priceFormat.Category3].Value.Trim(new[] { '"', ';' }).FirstLetterToUpper();
                        if (!string.IsNullOrWhiteSpace(child1) && !string.IsNullOrWhiteSpace(child2))
                        {
                            if (!children2.ContainsKey(child1))
                            {
                                children2.Add(child1, new Dictionary<string, string>());
                            }

                            if (!children2[child1].ContainsKey(child2))
                            {
                                children2[child1].Add(child2, photo);
                            }
                        }
                    }
                }
            }

            foreach (var parent in parents)
            {
                sb.AppendLine(string.Format("{0};{1};{2};", parent.Key, "Каталог", parent.Value));
                totalCount++;
            }

            foreach (var childrenPair in children1)
            {
                foreach (var child1 in childrenPair.Value)
                {
                    sb.AppendLine(string.Format("{0};{1};{2};", child1.Key, childrenPair.Key, child1.Value));
                    totalCount++;
                }
            }

            foreach (var childrenPair in children2)
            {
                foreach (var child2 in childrenPair.Value)
                {
                    sb.AppendLine(string.Format("{0};{1};{2};", child2.Key, childrenPair.Key, child2.Value));
                    totalCount++;
                }
            }

            File.WriteAllText(Path.GetDirectoryName(filePath) + "\\categories.csv", sb.ToString(), Encoding.UTF8);

            return totalCount;
        } 
    }
}
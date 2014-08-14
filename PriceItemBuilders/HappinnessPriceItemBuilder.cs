using System;
using System.Text.RegularExpressions;

using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;

namespace Supplier2Presta.PriceItemBuilders
{
    public class HappinessPriceItemBuilder : PriceItemBuilderBase, IPriceItemBuilder
    {
        private static readonly Regex LineRegex = new Regex(@"(""[^""]+"";)|(\d+ ?\d*;)|("""";)|([\w./:-]*;)|(""[^""]+""\r?)|(""""\r?)", RegexOptions.Compiled);

        public HappinessPriceItemBuilder(PriceFormat priceFormat, float? multiplicator, bool generateProperties)
            : base(priceFormat, multiplicator, generateProperties)
        {
        }

        public PriceItem Build(string line)
        {
            //Артикул;"Основная категория товара";"Подраздел категории товара";Наименование;Описание;Производитель;"Артикул производителя";"Цена (Розница)";"Цена (Опт)";"Можно купить";"На складе";"Время отгрузки";Размер;Цвет;aID;Материал;Батарейки;Упаковка;"Вес (брутто)";"Фотография маленькая до 150*150";"Фотография 1";"Фотография 2";"Фотография 3";"Фотография 4";"Фотография 5";Штрихкод
            //16;"Эротическая одежда";"Одежда из латекса";"Трусы мужские с вырезом для полового члена";"Трусы мужские с вырезом для полового члена. Размер M (46-48). ";Distra;"9650M BX GP";690.00;345.00;0;0;3;M;черный;20;латекс;;"картонная коробка";;http://feed.p5s.ru/images/small/small_16.jpg;http://feed.p5s.ru/images/big/16.jpg;;;;;
            var columns = LineRegex.Matches(line);

            if (columns.Count < 25 || columns.Count > 26)
            {
                throw new Exception(string.Format("Неправильный формат строки: {0}", line));
            }

            var result = this.Build(columns);
            result.Name = columns[this.PriceFormat.Name].Value.Trim(new[] { '"', ';' }).MakeSafeName();
            result.SupplierName = "happiness";
            result.Reference = "200" + columns[this.PriceFormat.Reference].Value.Trim(new[] { '"', ';' });
            result.Category = this.GetCategoryName(columns);
            result.Active = Convert.ToBoolean(Convert.ToInt32(columns[this.PriceFormat.Active].Value.Trim(new[] { '"', ';' })));
            result.Battery = columns[this.PriceFormat.Battery].Value.Trim(new[] { '"', ';' });
            result.Photo2 = columns[this.PriceFormat.Photo2].Value.Trim(new[] { '"', ';' });
            result.Photo3 = columns[this.PriceFormat.Photo3].Value.Trim(new[] { '"', ';' });
            result.Photo4 = columns[this.PriceFormat.Photo4].Value.Trim(new[] { '"', ';' });
            result.Photo5 = columns[this.PriceFormat.Photo5].Value.Trim(new[] { '"', ';' });
            result.Weight = columns[this.PriceFormat.Weight].Value.Trim(new[] { '"', ';' });

            return result;
        }

        private string GetCategoryName(MatchCollection columns)
        {
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

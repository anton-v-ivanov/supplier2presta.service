using NLog;
using Supplier2Presta.CategoryBuilders;
using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;
using Supplier2Presta.PriceItemBuilders;
using Supplier2Presta.Processors;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Supplier2Presta.Service
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        private static int currentCount;
        private static int totalCount;
        
        private const int NewPriceFileIsNotExists = -1;
        private const int NewPriceFileIsEmpty = -2;        

        static int Main(string[] args)
        {
            var priceEncoding = ConfigurationManager.AppSettings["price-encoding"];
            var oldPricePath = ConfigurationManager.AppSettings["old-price"];
            List<string> oldPriceLines = null;
            if (File.Exists(oldPricePath))
            {
                Log.Debug("Загрузка предыдущего прайса. Путь {0}", oldPricePath);

                oldPriceLines = File.ReadLines(oldPricePath, Encoding.GetEncoding(priceEncoding)).ToList();
                Log.Debug("Загружено {0} строк из предыдущего прайса", oldPriceLines.Count.ToString(CultureInfo.InvariantCulture));
            }

            var newPricePath = ConfigurationManager.AppSettings["new-price"];
            if(!File.Exists(newPricePath))
            {
                Log.Fatal(string.Format("Новый прайс пуст"));
                return NewPriceFileIsNotExists;
            }

            List<string> newPriceLines = File.ReadLines(newPricePath, Encoding.GetEncoding(priceEncoding)).ToList();
            Log.Debug("Загружено {0} строк из нового прайса", newPriceLines.Count.ToString(CultureInfo.InvariantCulture));

            if (newPriceLines.Count == 0)
            {
                Log.Fatal(string.Format("Новый прайс пуст"));
                return NewPriceFileIsEmpty;
            }

            int currentCount = 0;

            var oneFileCount = newPriceLines.Count;
            var split = Convert.ToBoolean(ConfigurationManager.AppSettings["file-split"]);
            if (split)
            {
                oneFileCount = Convert.ToInt32(Convert.ToInt32(ConfigurationManager.AppSettings["one-file-count"]));
            }

            var shortenings = new Dictionary<string, string>();
            var autoReplace = Convert.ToBoolean(ConfigurationManager.AppSettings["auto-replace"]);
            if (autoReplace)
            {
                var autoReplaces = File.ReadAllLines(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Words.txt", Encoding.UTF8);
                shortenings = Helper.GetShortenings(autoReplaces);
            }

            var multiplicator = float.Parse(ConfigurationManager.AppSettings["multiplicator"], CultureInfo.InvariantCulture);
            try
            {
                IPriceItemBuilder priceItemBuilder;
                PriceFormat priceFormat;
                if (rbSexsnab.Checked)
                {
                    var serializer = new XmlSerializer(typeof(PriceFormat));
                    using (var stream = new FileStream("sexsnab.xml", FileMode.Open))
                    {
                        priceFormat = (PriceFormat)serializer.Deserialize(stream);
                    }

                    priceItemBuilder = new SexsnabPriceItemBuilder(priceFormat, multiplicator, shortenings, rbFile.Checked);
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(PriceFormat));
                    using (var stream = new FileStream("happiness.xml", FileMode.Open))
                    {
                        priceFormat = (PriceFormat)serializer.Deserialize(stream);
                    }

                    priceItemBuilder = new HappinessPriceItemBuilder(priceFormat, multiplicator, rbFile.Checked);
                }

                IDiffer differ = new Differ(priceItemBuilder);
                IProcessor processor;
                if (rbApi.Checked)
                {
                    processor = new PriceWebServiceProcessor(differ);
                }
                else
                {
                    processor = new PriceFileProcessor(newPricePath, oneFileCount, differ);
                }

                ICategoryBuilder categoryBuilder = new CategoryFileBuilder(priceFormat, newPricePath);
                var categoryCount = categoryBuilder.Build(newPriceLines);

                processor.OnProductProcessed += OnProductProcessed;
                var task = new Task<Tuple<int, int, int>>(
                    () =>
                    {
                        OnProductProcessed("Построение диффа", GeneratedPriceType.None);
                        var diff = differ.GetDiff(newPriceLines, oldPriceLines);
                        totalCount = diff.DeletedItems.Count + diff.NewItems.Count + diff.SameItems.Count;
                        return processor.Process(diff);
                    });

                task.Start(TaskScheduler.FromCurrentSynchronizationContext());

                var result = task.Result;
                var count = newPriceLines.Count() - 1; // заголовок не считаем
                Log.Info("========================================================================");
                Log.Info(string.Format("Обработано {0} позиций. Создано {1} категорий.\r\nТовары: {2} одинаковых, {3} новых, {4} удалённых", count, categoryCount, result.Item1, result.Item2, result.Item3));

                Settings.Default.PrevPriceFile = newPricePath;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка обработки прайса", ex);
            }
        }

        private static void OnProductProcessed(string info, GeneratedPriceType currentGeneratedPriceType)
        {
            var priceTypeCategory = string.Empty;
            switch (currentGeneratedPriceType)
            {
                case GeneratedPriceType.None:
                    break;
                case GeneratedPriceType.NewItems:
                    priceTypeCategory = "Новые";
                    currentCount++;
                    break;
                case GeneratedPriceType.DeletedItems:
                    priceTypeCategory = "Удалённые";
                    currentCount++;
                    break;
                case GeneratedPriceType.SameItems:
                    priceTypeCategory = "Остатки";
                    currentCount++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("currentGeneratedPriceType");
            }

            if (currentGeneratedPriceType == GeneratedPriceType.None || currentCount % 10 == 0)
            {
                string text = string.IsNullOrEmpty(priceTypeCategory)
                    ? string.Format("{0}{1}", info, Environment.NewLine)
                    : string.Format("Категория: {0}   |  Счётчик {1} из {2}  |  {3}{4}", priceTypeCategory, currentCount, totalCount, info, Environment.NewLine);

                Log.Info(text);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using NLog;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.PriceItemBuilders;
using Supplier2Presta.Service.Processors;

namespace Supplier2Presta.Service
{
    public class Program
    {
        private const int Ok = 0;
        private const int NewPriceFileIsNotExists = -1;
        private const int NewPriceFileIsEmpty = -2;
        private const int InternalError = -3;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static int currentCount;
        private static int totalCount;

        private static List<PriceItem> NewProducts = new List<PriceItem>();

        public static int Main(string[] args)
        {
            var priceEncoding = ConfigurationManager.AppSettings["price-encoding"];

            var archiveDirectory = ConfigurationManager.AppSettings["archive-directory"];
            archiveDirectory = string.Format(@"{0}\{1}", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), archiveDirectory);

            var oldPriceLines = LoadOldPrice(archiveDirectory, priceEncoding);

            string newPriceUrl = ConfigurationManager.AppSettings["price-url"];
            float? multiplicator = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["multiplicator"]) 
                ? (float?)float.Parse(ConfigurationManager.AppSettings["multiplicator"], CultureInfo.InvariantCulture)
                : null;

            List<string> newPriceLines;
            string newPriceFileName;
            var newPriceLoadResult = TryLoadNewPrice(priceEncoding, newPriceUrl, out newPriceLines, out newPriceFileName);
            if (newPriceLoadResult != Ok)
            {
                throw new Exception("Невозможно загрузить новый прайс. Код ошибки: " + newPriceLoadResult);
            }

            var priceFormatFile = "happiness_short.xml";

            try
            {
                var result = Load(priceEncoding, newPriceLines, multiplicator, priceFormatFile, oldPriceLines);
                
                Log.Info("========================================================================");
                Log.Info(string.Format("Обработано {0} позиций. Товары: {1} обновлённых, {2} новых, {3} удалённых", result.Item1, result.Item2, result.Item3, result.Item4));
                
                SetLastPrice(newPriceFileName, archiveDirectory);
                
                string fullPriceUrl = ConfigurationManager.AppSettings["full-price-url"];

                newPriceLoadResult = TryLoadNewPrice(priceEncoding, fullPriceUrl, out newPriceLines, out newPriceFileName);
                if (newPriceLoadResult != Ok)
                {
                    throw new Exception("Невозможно загрузить новый прайс. Код ошибки: " + newPriceLoadResult);
                }

                var fullPriceAddResult = Load(priceEncoding, newPriceLines, multiplicator, "happiness.xml", null);
                
                return Ok;
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка обработки прайса", ex);
                return InternalError;
            }
        }

        private static Tuple<int, int, int, int> Load(string priceEncoding, List<string> newPriceLines, float? multiplicator, string priceFormatFile, List<string> oldPriceLines)
        {
            var serializer = new XmlSerializer(typeof(PriceFormat));
            PriceFormat priceFormat;
            using (var stream = new FileStream(priceFormatFile, FileMode.Open))
            {
                priceFormat = (PriceFormat)serializer.Deserialize(stream);
            }

            IPriceItemBuilder priceItemBuilder = new HappinessPriceItemBuilder(priceFormat, multiplicator);

            IDiffer differ = new Differ(priceItemBuilder);
            IProcessor processor = new PriceWebServiceProcessor(differ);
            processor.OnProductProcessed += OnProductProcessed;
            processor.OnNewProduct += OnNewProduct;

            var task = new Task<Tuple<int, int, int>>(
                () =>
                {
                    OnProductProcessed("Построение диффа", GeneratedPriceType.None);
                    var diff = differ.GetDiff(newPriceLines, oldPriceLines);
                    totalCount = diff.DeletedItems.Count + diff.NewItems.Count + diff.UpdatedItems.Count;
                    return processor.Process(diff);
                });

            task.Start();

            var result = task.Result;
            var count = newPriceLines.Count() - 1; // заголовок не считаем

            return new Tuple<int,int,int,int>(count, result.Item1, result.Item2, result.Item3);
        }

        private static int TryLoadNewPrice(string priceEncoding, string newPriceUrl, out List<string> newPrice, out string newPriceFileName)
        {
            newPrice = new List<string>();
            newPriceFileName = string.Format("price_{0}.csv", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(newPriceUrl, newPriceFileName);
            }

            if (!File.Exists(newPriceFileName))
            {
                Log.Fatal(string.Format("Новый прайс не существует"));
                return NewPriceFileIsNotExists;
            }

            newPrice = File.ReadLines(newPriceFileName, Encoding.GetEncoding(priceEncoding)).ToList();
            Log.Debug("Загружено {0} строк из нового прайса", newPrice.Count.ToString(CultureInfo.InvariantCulture));

            if (newPrice.Count > 0)
            {
                return Ok;
            }

            Log.Fatal(string.Format("Новый прайс пуст"));
            return NewPriceFileIsEmpty;
        }

        private static List<string> LoadOldPrice(string archiveDirectory, string priceEncoding)
        {
            var directory = new DirectoryInfo(archiveDirectory);
            var oldPriceFile = directory.GetFiles()
             .OrderByDescending(f => f.LastWriteTime)
             .FirstOrDefault();

            List<string> oldPriceLines = null;
            if (oldPriceFile != null)
            {
                Log.Debug("Загрузка предыдущего прайса. Путь {0}", oldPriceFile.Name);

                oldPriceLines = File.ReadLines(oldPriceFile.FullName, Encoding.GetEncoding(priceEncoding)).ToList();
                Log.Debug("Загружено {0} строк из предыдущего прайса", oldPriceLines.Count.ToString(CultureInfo.InvariantCulture));
            }
            return oldPriceLines;
        }

        private static void SetLastPrice(string newPricePath, string archiveDirectory)
        {
            var file = Path.GetFileName(newPricePath);
            File.Move(newPricePath, archiveDirectory + "\\" + file);
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

            string text = string.IsNullOrEmpty(priceTypeCategory)
                ? string.Format("{0}{1}", info, Environment.NewLine)
                : string.Format("Категория: {0}   |  Счётчик {1} из {2}  |  {3}{4}", priceTypeCategory, currentCount, totalCount, info, Environment.NewLine);

            Log.Info(text);
        }

        private static void OnNewProduct(PriceItem item)
        {
            Log.Warn("Продукт не существует и будет добавлен: {0}", item.ToString("Артикул: {{Reference}};"));
            NewProducts.Add(item);
        }
    }
}

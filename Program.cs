using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using NLog;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.PriceItemBuilders;
using Supplier2Presta.Service.Processors;
using System.Globalization;
using Supplier2Presta.Service.Loaders;

namespace Supplier2Presta.Service
{
    public class Program
    {
        private const int Ok = 0;
        private const int InternalError = -100;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static List<PriceItem> NewProducts = new List<PriceItem>();
        
        private static readonly string _priceEncoding;
        private static readonly string _archiveDirectory;
        private static readonly string _newPriceUrl;
        private static readonly float? _multiplicator;
        private static readonly string _fullPriceUrl;
        private static readonly IPriceLoader _oldPriceLoader;
        private static readonly IPriceLoader _newPriceLoader;

        static Program()
        {
            _priceEncoding = ConfigurationManager.AppSettings["price-encoding"];

            var dir = ConfigurationManager.AppSettings["archive-directory"];
            _archiveDirectory = string.Format(@"{0}\{1}", Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), dir);
            
            _newPriceUrl = ConfigurationManager.AppSettings["price-url"];
            
            _multiplicator = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["multiplicator"])
                ? (float?)float.Parse(ConfigurationManager.AppSettings["multiplicator"], CultureInfo.InvariantCulture)
                : null;
            
            _fullPriceUrl = ConfigurationManager.AppSettings["full-price-url"];

            _oldPriceLoader = new NewestFileSystemPriceLoader();
            _newPriceLoader = new InternetPriceLoader();
        }

        public static int Main(string[] args)
        {
            Log.Info("Запущено обновление прайсов");
            var result = CheckProductsUpdates();
            if (result != Ok)
            {
                return result;
            }

            var forceFull = args != null && args.Count() > 0 && args[0] == "full";
            if (!NewProducts.Any() && !forceFull)
            {
                return result;
            }
            
            Log.Info("Добавление новых продуктов");
            return CheckNewProducts();
        }

        private static int CheckProductsUpdates()
        {
            var oldPriceLoadResult = _oldPriceLoader.Load(_archiveDirectory, _priceEncoding);

            var newPriceLoadResult = _newPriceLoader.Load(_newPriceUrl, _priceEncoding);
            if (newPriceLoadResult.Result != PriceLoadResultType.Ok)
            {
                Log.Fatal("Невозможно загрузить новый прайс. Код ошибки: {0}", newPriceLoadResult.Result);
                return (int)newPriceLoadResult.Result;
            }

            var priceFormat = GetPriceFormat("happiness_short.xml");
            try
            {
                var diff = GetDiff(priceFormat, newPriceLoadResult.PriceLines, oldPriceLoadResult.PriceLines);

                IProcessor processor = new PriceWebServiceProcessor();
                processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems);
                processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);
                                
                var count = newPriceLoadResult.PriceLines.Count() - 1; // заголовок не считаем
                Log.Info(string.Format("Обработано {0} позиций. Товары: {1} обновлённых, {2} удалённых", count, diff.UpdatedItems.Count, diff.DeletedItems.Count));

                if (diff.DeletedItems.Any() || diff.NewItems.Any() || diff.UpdatedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, _archiveDirectory);
                }
                else
                {
                    File.Delete(newPriceLoadResult.FilePath);
                }

                return Ok;
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка обработки прайса", ex);
                File.Delete(newPriceLoadResult.FilePath);
                return InternalError;
            }
        }

        private static int CheckNewProducts()
        {
            Log.Debug("Загрузка полного прайса");
            var newPriceLoadResult = _newPriceLoader.Load(_fullPriceUrl, _priceEncoding);
            if (newPriceLoadResult.Result != PriceLoadResultType.Ok)
            {
                Log.Fatal("Невозможно загрузить новый прайс. Код ошибки: {0}", newPriceLoadResult.Result);
                return (int)newPriceLoadResult.Result;
            }
            
            var archiveDir = _archiveDirectory + "\\full";
            var oldPriceLoadResult = _oldPriceLoader.Load(archiveDir, _priceEncoding);

            var priceFormat = GetPriceFormat("happiness.xml");
            try
            {
                var diff = GetDiff(priceFormat, newPriceLoadResult.PriceLines, oldPriceLoadResult.PriceLines);
                
                var newItems = new Dictionary<string, PriceItem>();
                if (NewProducts.Any())
                {
                    foreach (var item in NewProducts)
                    {
                        if (diff.NewItems.ContainsKey(item.Reference))
                        {
                            newItems.Add(item.Reference, diff.NewItems[item.Reference]);
                        }
                    }
                    diff.NewItems = newItems;
                }

                if (diff.NewItems.Any())
                {
                    NewProducts.AddRange(diff.NewItems.Values);
                }
                
                IProcessor processor = new PriceWebServiceProcessor();
                processor.Process(diff.NewItems, GeneratedPriceType.NewItems);
                processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);

                var count = newPriceLoadResult.PriceLines.Count() - 1; // заголовок не считаем

                Log.Info(string.Format("Обработано {0} позиций. Товары: {1} новых, {2} удалённых", count, diff.NewItems.Count, diff.DeletedItems.Count));
                if (diff.NewItems.Any() || diff.DeletedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, archiveDir);
                }
                return Ok;
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка обработки прайса", ex);
                File.Delete(newPriceLoadResult.FilePath);
                return InternalError;
            }
        }

        private static Diff GetDiff(PriceFormat priceFormat, List<string> newPriceLines, List<string> oldPriceLines)
        {
            IPriceItemBuilder priceItemBuilder = new HappinessPriceItemBuilder(priceFormat, _multiplicator);
            IDiffer differ = new Differ(priceItemBuilder);

            Log.Debug("Построение диффа");
            var diff = differ.GetDiff(newPriceLines, oldPriceLines);
            return diff;
        }

        private static PriceFormat GetPriceFormat(string priceFormatFile)
        {
            var serializer = new XmlSerializer(typeof(PriceFormat));
            PriceFormat priceFormat;
            using (var stream = new FileStream(priceFormatFile, FileMode.Open))
            {
                priceFormat = (PriceFormat)serializer.Deserialize(stream);
            }
            return priceFormat;
        }

        private static void SetLastPrice(string newPricePath, string archiveDirectory)
        {
            var file = Path.GetFileName(newPricePath);
            File.Move(newPricePath, archiveDirectory + "\\" + file);
        }
    }
}

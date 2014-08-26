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
using Supplier2Presta.Service.Entities.Exceptions;

namespace Supplier2Presta.Service
{
    public class Program
    {
        private const int Ok = 0;
        private const int InternalError = -500;
        private const int ProcessAborted = -400;

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static bool _newProductsAppeared;
        
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
			_archiveDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), dir);
            
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
            Log.Info("Price update started");
            var result = CheckProductsUpdates();
            if (result != Ok && result != ProcessAborted)
            {
				Log.Info("Price update finished. ErrorCode: " + result);
                return result;
            }

            var forceFull = args != null && args.Count() > 0 && args[0] == "full";
            if (!_newProductsAppeared && !forceFull)
            {
				Log.Info("Price update finished");
                return result;
            }

			if (_newProductsAppeared) 
			{
				Log.Info("There are new products is price");
			}
			if (forceFull) 
			{
				Log.Info("Full price update forced");
			}

			result = CheckNewProducts();
			Log.Info("Price update finished");
			return result;
        }

        private static int CheckProductsUpdates()
        {
            var oldPriceLoadResult = _oldPriceLoader.Load(_archiveDirectory, _priceEncoding);

            var newPriceLoadResult = _newPriceLoader.Load(_newPriceUrl, _priceEncoding);
            if (newPriceLoadResult.Result != PriceLoadResultType.Ok)
            {
                Log.Fatal("Unable to load new price. Error code: {0}", newPriceLoadResult.Result);
                return (int)newPriceLoadResult.Result;
            }

            var priceFormat = GetPriceFormat("happiness_short.xml");
            try
            {
                var diff = GetDiff(priceFormat, newPriceLoadResult.PriceLines, oldPriceLoadResult.PriceLines);
                _newProductsAppeared = diff.NewItems.Any();

                IProcessor processor = new PriceWebServiceProcessor();
                processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems);
                processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);
                                
                var count = newPriceLoadResult.PriceLines.Count() - 1; // заголовок не считаем
                Log.Info(string.Format("{0} lines processed. {1} updated, {2} deleted", count, diff.UpdatedItems.Count, diff.DeletedItems.Count));

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
            catch (ProcessAbortedException)
            {
                Log.Error("Processing aborted");
                File.Delete(newPriceLoadResult.FilePath);
                return ProcessAborted;
            }
            catch (Exception ex)
            {
                Log.Error("Price processing error", ex);
                File.Delete(newPriceLoadResult.FilePath);
                return InternalError;
            }
        }

        private static int CheckNewProducts()
        {
            Log.Debug("Loading full price from supplier site");
            var newPriceLoadResult = _newPriceLoader.Load(_fullPriceUrl, _priceEncoding);
            if (newPriceLoadResult.Result != PriceLoadResultType.Ok)
            {
				Log.Fatal("Unable to load new price. Error code: {0}", newPriceLoadResult.Result);
                return (int)newPriceLoadResult.Result;
            }
            
			var archiveDir = Path.Combine(_archiveDirectory, "full");
            var oldPriceLoadResult = _oldPriceLoader.Load(archiveDir, _priceEncoding);

            var priceFormat = GetPriceFormat("happiness.xml");
            try
            {
                var diff = GetDiff(priceFormat, newPriceLoadResult.PriceLines, oldPriceLoadResult.PriceLines);
                
                IProcessor processor = new PriceWebServiceProcessor();
                processor.Process(diff.NewItems, GeneratedPriceType.NewItems);
                processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);

                var count = newPriceLoadResult.PriceLines.Count() - 1; // заголовок не считаем

                Log.Info(string.Format("{0} lines processed. {1} new, {2} deleted", count, diff.NewItems.Count, diff.DeletedItems.Count));
                if (diff.NewItems.Any() || diff.DeletedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, archiveDir);
                }
                return Ok;
            }
            catch (ProcessAbortedException)
            {
                Log.Error("Processing aborted");
                File.Delete(newPriceLoadResult.FilePath);
                return ProcessAborted;
            }
            catch (Exception ex)
            {
				Log.Error("Price processing error", ex);
                File.Delete(newPriceLoadResult.FilePath);
                return InternalError;
            }
        }
        
        #region Helper funcs

        private static Diff GetDiff(PriceFormat priceFormat, List<string> newPriceLines, List<string> oldPriceLines)
        {
            IPriceItemBuilder priceItemBuilder = new HappinessPriceItemBuilder(priceFormat, _multiplicator);
            IDiffer differ = new Differ(priceItemBuilder);

            Log.Debug("Building the diff");
            var diff = differ.GetDiff(newPriceLines, oldPriceLines);
            return diff;
        }

        private static PriceFormat GetPriceFormat(string priceFormatFile)
        {
            var serializer = new XmlSerializer(typeof(PriceFormat));
            PriceFormat priceFormat;
            var priceFormatFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), priceFormatFile);
            using (var stream = new FileStream(priceFormatFilePath, FileMode.Open))
            {
                priceFormat = (PriceFormat)serializer.Deserialize(stream);
            }
            return priceFormat;
        }

        private static void SetLastPrice(string newPricePath, string archiveDirectory)
        {
            var file = Path.GetFileName(newPricePath);
			File.Move(newPricePath, Path.Combine(archiveDirectory, file));
        }

        #endregion
    }
}

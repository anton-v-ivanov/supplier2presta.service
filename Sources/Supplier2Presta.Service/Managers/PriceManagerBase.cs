using NLog;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.Managers
{
    public class PriceManagerBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly IDiffer _differ;
        private readonly string _apiUrl;
        private readonly string _apiAccessToken;
        private readonly IRetailPriceBuilder _retailPriceBuilder;
        protected readonly SupplierElement _settings;
        protected string _archiveDirectory;

        public PriceManagerBase(SupplierElement settings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
        {
            _settings = settings;
            _apiUrl = apiUrl;
            _apiAccessToken = apiAccessToken;
            _retailPriceBuilder = retailPriceBuilder;
            _archiveDirectory = archiveDirectory;
            _differ = new Differ();
        }

        protected PriceUpdateResult Process(PriceLoadResult newPriceLoadResult, PriceLoadResult oldPriceLoadResult, PriceType type, bool forceUpdate)
        {
            try
            {
                Log.Debug("Building the diff");
                var diff = _differ.GetDiff(newPriceLoadResult.PriceItems, oldPriceLoadResult.PriceItems, forceUpdate);

                switch (type)
                {
                    case PriceType.Stock:
                        Log.Info("Price type is stock. New items are excluded from processing");
                        diff.NewItems.Clear();
                        break;
                    case PriceType.Full:
                        break;
                    case PriceType.Discount:
                        diff.NewItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = _settings.Discount; });
                        diff.UpdatedItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = _settings.Discount; });
                        break;
                    default:
                        break;
                }
                
                IProcessor processor = new PriceWebServiceProcessor(_apiUrl, _apiAccessToken);
                if (diff.NewItems.Any())
                {
                    Log.Debug("Building retail prices for new items");
                    diff.NewItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.Build(p));

                    Log.Debug("Adding items");
                    processor.Process(diff.NewItems, GeneratedPriceType.NewItems);
                }

                if (diff.UpdatedItems.Any())
                {
                    Log.Debug("Building retail prices for updated items");
                    diff.UpdatedItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.Build(p));

                    Log.Debug("Updating items");
                    processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems);
                }

                if (diff.DeletedItems.Any())
                {
                    Log.Debug("Deleting items");
                    processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);
                }

                Log.Info(string.Format("{0} lines processed. {1} updated, {2} added, {3} deleted", newPriceLoadResult.PriceItems.Count(), diff.UpdatedItems.Count, diff.NewItems.Count, diff.DeletedItems.Count));

                if (diff.DeletedItems.Any() || diff.NewItems.Any() || diff.UpdatedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, _archiveDirectory);
                }
                else
                {
                    File.Delete(newPriceLoadResult.FilePath);
                }

                return new PriceUpdateResult(PriceUpdateResultStatus.Ok);
            }
            catch (ProcessAbortedException)
            {
                Log.Error("Processing aborted");
                File.Delete(newPriceLoadResult.FilePath);
                return new PriceUpdateResult(PriceUpdateResultStatus.ProcessAborted);
            }
            catch (Exception ex)
            {
                Log.Error("Price processing error", ex);
                File.Delete(newPriceLoadResult.FilePath);
                return new PriceUpdateResult(PriceUpdateResultStatus.InternalError);
            }
        }

        private static void SetLastPrice(string newPricePath, string archiveDirectory)
        {
            var file = Path.GetFileName(newPricePath);
            File.Move(newPricePath, Path.Combine(archiveDirectory, file));
        }
    }
}

using NLog;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.Multiplicators;
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
        
        protected string _archiveDirectory;

        public PriceManagerBase(string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
        {
            _apiUrl = apiUrl;
            _apiAccessToken = apiAccessToken;
            _retailPriceBuilder = retailPriceBuilder;

            _differ = new Differ();
        }

        public PriceUpdateResult ProcessShortPrice(PriceLoadResult newPriceLoadResult, PriceLoadResult oldPriceLoadResult)
        {
            try
            {
                Log.Debug("Building the diff");
                var diff = _differ.GetDiff(newPriceLoadResult.PriceItems, oldPriceLoadResult.PriceItems);

                var newProductsAppeared = diff.NewItems.Any();

                IProcessor processor = new PriceWebServiceProcessor(_apiUrl, _apiAccessToken);

                Log.Debug("Building retail prices");
                diff.UpdatedItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.Build(p));

                if (diff.UpdatedItems.Any())
                {
                    Log.Debug("Updating items");
                    processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems);
                }

                if (diff.DeletedItems.Any())
                {
                    Log.Debug("Deleting items");
                    processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);
                }

                Log.Info(string.Format("{0} lines processed. {1} updated, {2} deleted", newPriceLoadResult.PriceItems.Count(), diff.UpdatedItems.Count, diff.DeletedItems.Count));

                if (diff.DeletedItems.Any() || diff.NewItems.Any() || diff.UpdatedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, _archiveDirectory);
                }
                else
                {
                    File.Delete(newPriceLoadResult.FilePath);
                }

                return new PriceUpdateResult(PriceUpdateResultStatus.Ok, newProductsAppeared);
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

        public PriceUpdateResult ProcessFullPrice(PriceLoadResult newPriceLoadResult, PriceLoadResult oldPriceLoadResult, string archiveDir)
        {
            try
            {
                Log.Debug("Building the diff");
                var diff = _differ.GetDiff(newPriceLoadResult.PriceItems, oldPriceLoadResult.PriceItems);

                IProcessor processor = new PriceWebServiceProcessor(_apiUrl, _apiAccessToken);

                Log.Debug("Building retail prices");
                diff.NewItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.Build(p));
                
                Log.Debug("Adding items");
                processor.Process(diff.NewItems, GeneratedPriceType.NewItems);

                Log.Debug("Deleting items");
                processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems);

                Log.Info(string.Format("{0} lines processed. {1} new, {2} deleted", newPriceLoadResult.PriceItems.Count(), diff.NewItems.Count, diff.DeletedItems.Count));
                if (diff.NewItems.Any() || diff.DeletedItems.Any())
                {
                    SetLastPrice(newPriceLoadResult.FilePath, archiveDir);
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

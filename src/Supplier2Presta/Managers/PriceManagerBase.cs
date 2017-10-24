using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;
using Supplier2Presta.Entities.Exceptions;
using Supplier2Presta.PriceBuilders;
using Supplier2Presta.ShopApiProcessors;

namespace Supplier2Presta.Managers
{
    public abstract class PriceManagerBase : IPriceManager
    {
        public string PriceUrl { get; }
        public int DiscountValue { get; }

        private readonly IProcessor _processor;
        private readonly IRetailPriceBuilder _retailPriceBuilder;
        protected string ArchiveDirectory;

        protected PriceManagerBase(IProcessor processor, SupplierSettings supplierSettings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder)
        {
            PriceUrl = supplierSettings.Url;
            DiscountValue = supplierSettings.Discount;
            _processor = processor;
            _retailPriceBuilder = retailPriceBuilder;
            ArchiveDirectory = archiveDirectory;
        }

        public abstract LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate);

        public async Task<PriceUpdateResult> Process(Diff diff, PriceType priceType)
        {
            try
            {
                switch (priceType)
                {
                    case PriceType.Stock:
                        Log.Information("Price type is stock. New items are excluded from processing");
                        diff.NewItems.Clear();
                        break;
                    case PriceType.Full:
                        break;
                    case PriceType.Discount:
                        diff.NewItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = DiscountValue; });
                        diff.UpdatedItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = DiscountValue; });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(priceType), priceType, null);
                }
                
                if (diff.NewItems.Any())
                {
                    Log.Debug("Building retail prices for new items");
                    diff.NewItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.GetRetailPrice(p));

                    Log.Debug("Adding items");
                    await _processor.Process(diff.NewItems, GeneratedPriceType.NewItems, priceType);
                }

                if (diff.UpdatedItems.Any())
                {
                    Log.Debug("Building retail prices for updated items");
                    diff.UpdatedItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.GetRetailPrice(p));

                    Log.Debug("Updating items");
                    await _processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems, priceType);
                }

                if (diff.DeletedItems.Any())
                {
                    Log.Debug("Deleting items");
                    await _processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems, priceType);
                }

                return new PriceUpdateResult(PriceUpdateResultStatus.Ok);
            }
            catch (PhotoLoadException)
            {
                return new PriceUpdateResult(PriceUpdateResultStatus.PhotoLoadFailed);
            }
            catch (ProcessAbortedException)
            {
                return new PriceUpdateResult(PriceUpdateResultStatus.ProcessAborted);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Price processing error");
                return new PriceUpdateResult(PriceUpdateResultStatus.InternalError);
            }
        }
    }
}

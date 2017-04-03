using System;
using System.Linq;
using NLog;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.ShopApiProcessors;

namespace Supplier2Presta.Service.Managers
{
    public abstract class PriceManagerBase : IPriceManager
    {
        public string PriceUrl { get; }
        public int DiscountValue { get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string _apiUrl;
        private readonly string _apiAccessToken;
        private readonly IRetailPriceBuilder _retailPriceBuilder;
        protected string ArchiveDirectory;

        protected PriceManagerBase(string priceUrl, int discountValue, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken)
        {
            PriceUrl = priceUrl;
            DiscountValue = discountValue;
            _apiUrl = apiUrl;
            _apiAccessToken = apiAccessToken;
            _retailPriceBuilder = retailPriceBuilder;
            ArchiveDirectory = archiveDirectory;
        }

        public abstract LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate);

        public PriceUpdateResult Process(Diff diff, PriceType priceType)
        {
            try
            {
                switch (priceType)
                {
                    case PriceType.Stock:
                        Log.Info("Price type is stock. New items are excluded from processing");
                        diff.NewItems.Clear();
                        break;
                    case PriceType.Full:
                        break;
                    case PriceType.Discount:
                        diff.NewItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = DiscountValue; });
                        diff.UpdatedItems.Values.ToList().ForEach(p => { p.OnSale = true; p.DiscountValue = DiscountValue; });
                        break;
                }
                
                IProcessor processor = new PriceWebServiceProcessor(_apiUrl, _apiAccessToken);
                if (diff.NewItems.Any())
                {
                    Log.Debug("Building retail prices for new items");
                    diff.NewItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.GetRetailPrice(p));

                    Log.Debug("Adding items");
                    processor.Process(diff.NewItems, GeneratedPriceType.NewItems, priceType);
                }

                if (diff.UpdatedItems.Any())
                {
                    Log.Debug("Building retail prices for updated items");
                    diff.UpdatedItems.Values.ToList().ForEach(p => p.RetailPrice = _retailPriceBuilder.GetRetailPrice(p));

                    Log.Debug("Updating items");
                    processor.Process(diff.UpdatedItems, GeneratedPriceType.SameItems, priceType);
                }

                if (diff.DeletedItems.Any())
                {
                    Log.Debug("Deleting items");
                    processor.Process(diff.DeletedItems, GeneratedPriceType.DeletedItems, priceType);
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
                Log.Error("Price processing error", ex);
                return new PriceUpdateResult(PriceUpdateResultStatus.InternalError);
            }
        }
    }
}

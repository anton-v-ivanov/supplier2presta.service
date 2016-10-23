using Supplier2Presta.Service.Loaders;

namespace Supplier2Presta.Service.Entities
{
    public class LoadUpdatesResult
    {
        public PriceLoadResult NewPriceLoadResult { get; }
        public PriceLoadResult OldPriceLoadResult { get; }
        public bool IsSuccess { get; }

        public LoadUpdatesResult(PriceLoadResult newPriceLoadResult, PriceLoadResult oldPriceLoadResult, bool isSuccess)
        {
            NewPriceLoadResult = newPriceLoadResult;
            OldPriceLoadResult = oldPriceLoadResult;
            IsSuccess = isSuccess;
        }
    }
}
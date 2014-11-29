namespace Supplier2Presta.Service.Entities
{
    public enum PriceUpdateResultStatus
    {
        Ok = 0,
        PriceLoadFail = 1,
        InternalError = 500,
        ProcessAborted = 400,
    }

    public class PriceUpdateResult
    {
        public PriceUpdateResultStatus Status { get; set; }
        
        public PriceUpdateResult(PriceUpdateResultStatus status)
        {
            Status = status;
        }
    }
}

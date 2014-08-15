namespace Supplier2Presta.Service.Entities
{
    public class EventDelegates
    {
        public delegate void ProcessEventDelegate(string info, GeneratedPriceType currentGeneratedPriceType);
    }
}
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.Managers
{
    public interface IPriceManager
    {
        PriceUpdateResult CheckUpdates(PriceType type, bool forceUpdate);
    }
}

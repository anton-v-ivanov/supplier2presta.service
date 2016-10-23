using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.Managers
{
    public interface IPriceManager
    {
        LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate);

        PriceUpdateResult Process(Diff diff, PriceType priceType);
    }
}

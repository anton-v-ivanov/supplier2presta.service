using System.Threading.Tasks;
using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;

namespace Supplier2Presta.Managers
{
    public interface IPriceManager
    {
        LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate);

        Task<PriceUpdateResult> Process(Diff diff, PriceType priceType);
    }
}

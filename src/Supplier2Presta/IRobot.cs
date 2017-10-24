using System.Threading;
using System.Threading.Tasks;
using Supplier2Presta.Entities;

namespace Supplier2Presta
{
    public interface IRobot
    {
        Task<PriceUpdateResultStatus> ProcessPrice(CancellationTokenSource cancellationToken);
    }
}
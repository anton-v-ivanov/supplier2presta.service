using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;

namespace Supplier2Presta.ShopApiProcessors
{
    public interface IProductRemover
    {
        Task Remove(product product);
    }
}
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface IImageProcessor
    {
        Task<image> GetImageValue(string url, product product);
    }
}
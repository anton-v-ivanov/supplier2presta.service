using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
    public interface IFeatureProcessor
    {
        Task<product_feature_value> GetFeatureValue(string value, product_feature feature);
    }
}
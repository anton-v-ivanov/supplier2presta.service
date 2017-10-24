using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Serilog;

namespace Supplier2Presta.ShopApiProcessors
{
	public class ProductRemover : IProductRemover
    {
		private readonly IShopApiFactory _apiFactory;

		public ProductRemover(IShopApiFactory apiFactory)
		{
			_apiFactory = apiFactory;
		}

		public async Task Remove(product product)
		{
			Log.Debug("Disabling product {0}", product.id);
			product.active = 0;
			await _apiFactory.ProductFactory.Update(product);
			Log.Information("Product disabled: {0}", product.id);
		}
	}
}

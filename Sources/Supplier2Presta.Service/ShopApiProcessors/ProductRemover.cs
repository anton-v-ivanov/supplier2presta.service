using Bukimedia.PrestaSharp.Entities;
using NLog;

namespace Supplier2Presta.Service.ShopApiProcessors
{
	public class ProductRemover
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		private readonly ShopApiFactory _apiFactory;

		public ProductRemover(ShopApiFactory apiFactory)
		{
			_apiFactory = apiFactory;
		}

		public void Remove(product product)
		{
			Log.Debug("Disabling product {0}", product.id);
			product.active = 0;
			_apiFactory.ProductFactory.Update(product);
			Log.Info("Product disabled: {0}", product.id);
		}
	}
}

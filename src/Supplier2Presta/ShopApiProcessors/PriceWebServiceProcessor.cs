using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Supplier2Presta.Entities;
using Supplier2Presta.Entities.Exceptions;

namespace Supplier2Presta.ShopApiProcessors
{
	public class PriceWebServiceProcessor : IProcessor
	{
		private readonly IShopApiFactory _apiFactory;
		private readonly IProductUpdater _productUpdater;
		private readonly IProductRemover _productRemover;
		private readonly IProductCreator _productCreator;

		public PriceWebServiceProcessor(IShopApiFactory apiFactory, 
            IProductUpdater productUpdater, 
            IProductRemover productRemover, 
            IProductCreator productCreator)
		{
		    _apiFactory = apiFactory;
		    _productUpdater = productUpdater;
		    _productRemover = productRemover;
		    _productCreator = productCreator;
		}

		public async Task Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType, PriceType processingPriceType)
		{
			var currentCount = 0;
			var photoLoadErrorsOccured = false;

			foreach(var kvp in priceItems)
			{
			    currentCount++;

			    var item = kvp.Value;
				Log.Information("Processing product {current} of {count} Reference: {reference}", currentCount, priceItems.Count, item.Reference);

				var filter = new Dictionary<string, string> { { "reference", item.Reference } };
				var existingProd = await _apiFactory.ProductFactory.GetByFilter(filter, null, null);

				switch (generatedPriceType)
				{
					case GeneratedPriceType.NewItems:
						if (existingProd == null || !existingProd.Any())
						{
							try
							{
								await _productCreator.Create(item);
							}
							catch (PhotoLoadException)
							{
								photoLoadErrorsOccured = true;
							}
							catch (Exception ex)
							{
								Log.Error(ex, "Product add error. Reference: {reference}", item.Reference);
							}
						}
						else
						{
							try
							{
								await _productUpdater.Update(existingProd.First(), item, processingPriceType);
							}
							catch (Exception ex)
							{
								Log.Error(ex, "Balance update error. Reference: {reference}", item.Reference);
								await _productRemover.Remove(existingProd.First());
							}
						}
						break;
					case GeneratedPriceType.SameItems:
						if (existingProd == null || !existingProd.Any())
						{
							Log.Warning("Product does't exists. It will be added later. Reference: {0}", item.Reference);
						}
						else
						{
							try
							{
								await _productUpdater.Update(existingProd.First(), item, processingPriceType);
							}
							catch (Exception ex)
							{
								Log.Error(ex, "Update error. Reference: {reference}", item.Reference);
								await _productRemover.Remove(existingProd.First());
							}
						}
						break;
					case GeneratedPriceType.DeletedItems:
						if (existingProd != null && existingProd.Any())
						{
							try
							{
								if (processingPriceType != PriceType.Discount)
								{
									await _productRemover.Remove(existingProd.First());
								}
								else
								{
									await _productUpdater.RemoveDiscountInfo(item, existingProd.First());
								}
							}
							catch (Exception ex)
							{
								Log.Error(ex, "Disable product error. Reference: {reference}", item.Reference);
							}
						}
						break;
				}
			}
			if (photoLoadErrorsOccured)
			{
				throw new PhotoLoadException();
			}
		}
	}
}

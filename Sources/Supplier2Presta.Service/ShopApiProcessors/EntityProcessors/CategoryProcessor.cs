using System;
using System.Collections.Generic;
using Bukimedia.PrestaSharp.Entities;
using Supplier2Presta.Service.Entities;

namespace Supplier2Presta.Service.ShopApiProcessors.EntityProcessors
{
	class CategoryProcessor
	{
		private readonly ShopApiFactory _apiFactory;

		public CategoryProcessor(ShopApiFactory apiFactory)
		{
			_apiFactory = apiFactory;
		}

		internal List<category> GetCategories(PriceItem priceItem)
		{
			var result = new List<category>();
			foreach (var category in priceItem.Categories)
			{
				var filter = _apiFactory.CategoryFactory.GetByFilter(new Dictionary<string, string>{{"name", category.SubName }}, null, null);
				if (filter == null)
					throw new Exception("Unknown category");
				result.AddRange(filter);
			}
			return result;
		}
	}
}

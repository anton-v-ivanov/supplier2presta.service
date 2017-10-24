using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Serilog;
using Supplier2Presta.Entities;
using Supplier2Presta.Helpers;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;

namespace Supplier2Presta.ShopApiProcessors.EntityProcessors
{
	public class CategoryProcessor: ICategoryProcessor
    {
		private readonly IShopApiFactory _apiFactory;

		public CategoryProcessor(IShopApiFactory apiFactory)
		{
			_apiFactory = apiFactory;
		}

		public async Task<List<category>> GetCategories(PriceItem priceItem)
		{
			var result = new List<category>();
			foreach (var category in priceItem.Categories)
			{
				var categories = await _apiFactory.CategoryFactory.GetByFilter(new Dictionary<string, string>{{"name", category.SubName }}, null, null);
			    if (!categories.Any())
			    {
                    Log.Warning($"Adding new category '{category.SubName}', you should move it to right place!");
			        var link = TranslitHelper.Front(category.SubName).ToLowerInvariant();
                    var cat = new category
			        {
                        id_parent = 2,
			            name = new List<language> {new language(1, category.SubName)},
                        meta_title = new List<language> { new language(1, category.SubName) },
                        meta_description = new List<language> { new language(1, $"купить {category.SubName} дёшево") },
                        link_rewrite = new List<language> { new language(1, link) },
                        active = 1
                    };

                    cat = await _apiFactory.CategoryFactory.Add(cat);
                    categories.Add(cat);
			    }
			    result.AddRange(categories);
			}
			return result;
		}
	}
}

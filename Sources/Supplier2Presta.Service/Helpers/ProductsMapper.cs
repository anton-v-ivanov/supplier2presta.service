using System;
using System.Collections.Generic;
using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Entities.AuxEntities;
using Supplier2Presta.Service.Entities;
using category = Bukimedia.PrestaSharp.Entities.category;
using image = Bukimedia.PrestaSharp.Entities.image;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;
using product = Bukimedia.PrestaSharp.Entities.product;
using product_feature = Bukimedia.PrestaSharp.Entities.AuxEntities.product_feature;
using product_feature_value = Bukimedia.PrestaSharp.Entities.product_feature_value;
using stock_available = Bukimedia.PrestaSharp.Entities.stock_available;
using supplier = Bukimedia.PrestaSharp.Entities.supplier;

namespace Supplier2Presta.Service.Helpers
{
	internal static class ProductsMapper
	{
		internal static product Create(PriceItem item)
		{
			var result = new product
			{
				active = Convert.ToInt32(item.Active),
				ean13 = item.Ean13,
				reference = item.Reference,
				supplier_reference = item.SupplierReference,
				price = Convert.ToDecimal(item.RetailPrice),
				wholesale_price = Convert.ToDecimal(item.WholesalePrice),
				show_price = 1,
				redirect_type = "404",
				id_shop_default = 1,
				available_for_order = 1,
				advanced_stock_management = 0,
				id_tax_rules_group = 1,
				minimal_quantity = 1,
				weight = !string.IsNullOrWhiteSpace(item.Weight) ? Convert.ToDecimal(item.Weight) : 0,
				associations = new AssociationsProduct(),
				name = new List<language> { new language(1, item.Name) },
				description = new List<language> { new language(1, item.Description) },
				description_short = new List<language> { new language(1, item.ShortDescription) }
			};

			return result;
		}

		internal static product MapSupplier(product product, supplier supplier)
		{
			product.id_supplier = supplier.id;
			return product;
		}

		internal static product MapCategories(product product, List<category> categories)
		{
			product.id_category_default = categories[0].id;
			product.associations.categories = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.category>();
			foreach (var category in categories)
				product.associations.categories.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.category(category.id.Value));

			return product;
		}

		internal static product MapFeature(product product, product_feature_value featureValue)
		{
			if (featureValue == null)
			{
				return product;
			}

			product.associations.product_features.Add(
				new product_feature
				{
					id = featureValue.id_feature.Value,
					id_feature_value = featureValue.id.Value
				});
			return product;
		}

		internal static product MapStock(product product, stock_available stock)
		{
			product.associations.stock_availables = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.stock_available>
			{
				new Bukimedia.PrestaSharp.Entities.AuxEntities.stock_available
				{
					id = stock.id.Value,
					id_product_attribute = stock.id_product_attribute.Value
				}
			};
			return product;
		}

		internal static product MapImage(product product, image image)
		{
			if (image == null)
			{
				return product;
			}

			if (product.associations.images == null)
			{
				product.associations.images = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.image>();
			}

			product.associations.images.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.image { id = image.id });
			return product;
		}

		internal static product MapManufacturer(product product, manufacturer manufacturerValue)
		{
			product.id_manufacturer = manufacturerValue.id;
			return product;
		}

		internal static product FillMetaInfo(PriceItem priceItem, product product)
		{
			if (string.IsNullOrWhiteSpace(priceItem.Name))
				return product;

			product.meta_title = new List<language> { new language(1, priceItem.Name) };
			product.meta_description = new List<language> { new language(1, string.Format("Купить {0} в Москве", priceItem.Name)) };
			product.meta_keywords = new List<language>
			{
				new language(1, string.Format("Купить {0} в Москве", priceItem.Name))
			};
			return product;
		}

		internal static product MapCombination(product product, combination combination)
		{
			if (combination == null)
			{
				return product;
			}

			product.associations.combinations.Add(
				new combinations
				{
					id = combination.id.Value
				});
			return product;

		}
	}
}

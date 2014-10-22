using System;
using System.Collections.Generic;
using System.Linq;

using Bukimedia.PrestaSharp.Entities;

using Supplier2Presta.Service.Entities;

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
                                 associations = new Bukimedia.PrestaSharp.Entities.AuxEntities.AssociationsProduct(),
                                 name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, item.Name) },
                                 description = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, item.Description) },
                                 description_short = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, item.ShortDescription) }
                             };

            return result;
        }
        
        internal static product MapSupplier(product product, supplier supplier)
        {
            product.id_supplier = supplier.id;
            return product;
        }

        internal static product MapCategory(product product, category category)
        {
            product.id_category_default = category.id;
            product.associations.categories = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.category>
            {
                new Bukimedia.PrestaSharp.Entities.AuxEntities.category(category.id.Value)
            };
            return product;
        }

        internal static product MapFeature(product product, product_feature_value featureValue)
        {
            if (featureValue == null)
            {
                return product;
            }

            product.associations.product_features.Add(
                new Bukimedia.PrestaSharp.Entities.AuxEntities.product_feature
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

            product.associations.images.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.image { id = image.id } );
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

            product.meta_title = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, priceItem.Name) };
            product.meta_description = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, string.Format("Купить {0} в Москве", priceItem.Name)) };
            var words = priceItem.Name.Split(new char[] { ' ' }).Where(s => s.Length > 3);
            if (words.Any())
            {
                product.meta_keywords = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language>();
                foreach (var word in words)
                {
                    product.meta_keywords.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, word));
                }
            }
            return product;
        }
    }
}

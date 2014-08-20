using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;

using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Helpers;
using NLog;

namespace Supplier2Presta.Service.Processors
{
    public class PriceWebServiceProcessor : IProcessor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly ShopApiFactory apiFactory;

        public PriceWebServiceProcessor()
        {
            this.apiFactory = new ShopApiFactory();
        }

        public void Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType)
        {
            Log.Debug("Подключение к API");

            this.apiFactory.InitFactories("http://dirty-dreams.ru/api", "4MFF2R1ZFA4DXSYLDSKP8MK7V7W83KWL");

            ProcessDiff(priceItems, generatedPriceType);
        }
        
        private void ProcessDiff(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType)
        {
            var priceTypeCategory = string.Empty;
            int currentCount = 0;

            foreach (var item in priceItems.Values)
            {
                currentCount++;
                var filter = new Dictionary<string, string> { { "reference", item.Reference } };
                var existingProd = this.apiFactory.ProductFactory.GetByFilter(filter, null, null);

                switch (generatedPriceType)
                {
                    case GeneratedPriceType.NewItems:
                        priceTypeCategory = "Новые";
                        if (existingProd == null || !existingProd.Any())
                        {
                            try
                            {
                                AddNewProduct(item);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ошибка при добавлении продукта Артикул: {0}; {1}", item.ToString("{{Reference}}"), ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                this.UpdateProductBalance(item, existingProd.First());
                                this.UpdateProductPriceAndActivity(item, existingProd.First());
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ошибка при обновлении продукта Артикул: {0}; {1}", item.ToString("{{Reference}}"), ex);
                            }
                        }
                        break;
                    case GeneratedPriceType.SameItems:
                        priceTypeCategory = "Остатки";
                        if (existingProd == null || !existingProd.Any())
                        {
                            Log.Warn("Продукт не существует. Он будет добавлен позже. Артикул: {0}", item.ToString("{{Reference}};"));
                        }
                        else
                        {
                            try
                            {
                                this.UpdateProductBalance(item, existingProd.First());
                                this.UpdateProductPriceAndActivity(item, existingProd.First());
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ошибка при обновлении продукта Артикул: {0}; {1}", item.ToString("{{Reference}}"), ex);
                            }
                        }

                        break;
                    case GeneratedPriceType.DeletedItems:
                        priceTypeCategory = "Удалённые";
                        if (existingProd != null && existingProd.Any())
                        {
                            try
                            {
                                this.DisableProduct(existingProd.First());
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Ошибка при удалении продукта Артикул: {0}; {1}", item.ToString("{{Reference}}"), ex);
                            }
                        }
                        break;
                }

                Log.Debug("Категория: {0}   |  Счётчик {1} из {2}  |  Артикул: {3}", priceTypeCategory, currentCount, priceItems.Count, item.Reference);                
            }
        }

        private void UpdateProductBalance(PriceItem priceItem, product product)
        {
            var stock = this.GetStockValue(priceItem, product);
            if (stock.quantity != priceItem.Balance)
            {
                stock.quantity = priceItem.Balance;
                this.apiFactory.StockFactory.Update(stock);
            }
        }

        private void UpdateProductPriceAndActivity(PriceItem priceItem, product product)
        {
            if (product.price != Convert.ToDecimal(priceItem.RetailPrice) || product.wholesale_price != Convert.ToDecimal(priceItem.WholesalePrice) 
                || product.active != Convert.ToInt32(priceItem.Active))
            {
                product.price = Convert.ToDecimal(priceItem.RetailPrice);
                product.wholesale_price = Convert.ToDecimal(priceItem.WholesalePrice);
                product.active = Convert.ToInt32(priceItem.Active);
                this.apiFactory.ProductFactory.Update(product);
            }
        }

        private void AddNewProduct(PriceItem priceItem)
        {
            Log.Info("Добавление продукта. Артикул: {0}", priceItem.ToString("{{Reference}};"));
            var product = ProductsMapper.Create(priceItem);

            var category = this.GetCategoryValue(priceItem);
            product = ProductsMapper.MapCategory(product, category);

            var supplier = this.apiFactory.Suppliers.First(s => s.name.Equals(priceItem.SupplierName, StringComparison.CurrentCultureIgnoreCase));
            product = ProductsMapper.MapSupplier(product, supplier);

            var featureValue = this.GetFeatureValue(priceItem.Size, this.apiFactory.SizeFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Color, this.apiFactory.ColorFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Material, this.apiFactory.MaterialFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Country, this.apiFactory.CountryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Packing, this.apiFactory.PackingFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Length, this.apiFactory.LengthFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Diameter, this.apiFactory.DiameterFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Battery, this.apiFactory.BatteryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            var manufacturerValue = this.GetManufacturerValue(priceItem, product);
            product = ProductsMapper.MapManufacturer(product, manufacturerValue);

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
            // Добавление продукта
            product = this.apiFactory.ProductFactory.Add(product);
            
            this.GetProductSupplierValue(priceItem, product, supplier);

            var image1 = this.GetImageValue(priceItem.Photo1, product);
            product = ProductsMapper.MapImage(product, image1);

            var image2 = this.GetImageValue(priceItem.Photo2, product);
            product = ProductsMapper.MapImage(product, image2);

            var image3 = this.GetImageValue(priceItem.Photo3, product);
            product = ProductsMapper.MapImage(product, image3);

            var image4 = this.GetImageValue(priceItem.Photo4, product);
            product = ProductsMapper.MapImage(product, image4);

            var image5 = this.GetImageValue(priceItem.Photo5, product);
            product = ProductsMapper.MapImage(product, image5);

            this.UpdateProductBalance(priceItem, product);
        }

        private manufacturer GetManufacturerValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Manufacturer } };
            var manufacturers = this.apiFactory.ManufacturerFactory.GetByFilter(filter, null, null);

            if (manufacturers == null || !manufacturers.Any())
            {
                var manufacturer = new manufacturer
                {
                    name = priceItem.Manufacturer,
                    active = 1,
                };
                return this.apiFactory.ManufacturerFactory.Add(manufacturer);
            }
            return manufacturers.First();
        }

        private product_supplier GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var supplier = this.apiFactory.ProductSupplierFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (supplier == null)
            {
                supplier = new product_supplier
                {
                    id_currency = 1,
                    id_product = product.id,
                    id_product_attribute = 0,
                    id_supplier = supplierFeature.id,
                    product_supplier_reference = priceItem.SupplierReference,
                    product_supplier_price_te = Convert.ToDecimal(priceItem.WholesalePrice)
                };
                supplier = this.apiFactory.ProductSupplierFactory.Add(supplier);
            }

            return supplier;
        }

        private image GetImageValue(string url, product product)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (var client = new WebClient())
            {
                try
                {
                    var bytes = client.DownloadData(url);
                    return this.apiFactory.ImageFactory.AddProductImage(product.id.Value, bytes);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private category GetCategoryValue(PriceItem priceItem)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Category } };

            return this.apiFactory.CategoryFactory.GetByFilter(filter, null, null).First();
        }

        private product_feature_value GetFeatureValue(string value, product_feature feature)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var filter = new Dictionary<string, string> { { "id_feature", feature.id.Value.ToString(CultureInfo.InvariantCulture) }, { "value", value }, };

            var featureValues = this.apiFactory.FeatureValuesFactory.GetByFilter(filter, null, null);
            product_feature_value featureValue;
            if (featureValues == null || !featureValues.Any())
            {
                featureValue = new product_feature_value
                {
                    id_feature = feature.id,
                    value = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, value) }
                };
                featureValue = this.apiFactory.FeatureValuesFactory.Add(featureValue);
            }
            else
            {
                featureValue = featureValues.First();
            }

            return featureValue;
        }

        private stock_available GetStockValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var stock = this.apiFactory.StockFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (stock == null)
            {
                stock = new stock_available
                {
                    id_product = product.id,
                    quantity = priceItem.Balance
                };
                stock = this.apiFactory.StockFactory.AddList(new List<stock_available> { stock }).First();
            }

            return stock;
        }

        private void DisableProduct(product product)
        {
            product.active = 0;
            this.apiFactory.ProductFactory.Update(product);
        }
    }
}

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
using Supplier2Presta.Service.Entities.Exceptions;

namespace Supplier2Presta.Service.Processors
{
    public class PriceWebServiceProcessor : IProcessor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly ShopApiFactory _apiFactory;
        private string _apiUrl;
        private string _accessToken;

        public PriceWebServiceProcessor(string apiUrl, string accessToken)
        {
            _apiFactory = new ShopApiFactory();
            _apiUrl = apiUrl;
            _accessToken = accessToken;
        }

        public void Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType)
        {
            Log.Debug("Connecting to API");

            _apiFactory.InitFactories(_apiUrl, _accessToken);

            ProcessDiff(priceItems, generatedPriceType);
        }
        
        private void ProcessDiff(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType)
        {
            int currentCount = 0;

            foreach (var item in priceItems.Values)
            {
                currentCount++;
                var filter = new Dictionary<string, string> { { "reference", item.Reference } };
                var existingProd = _apiFactory.ProductFactory.GetByFilter(filter, null, null);

                switch (generatedPriceType)
                {
                    case GeneratedPriceType.NewItems:
                        if (existingProd == null || !existingProd.Any())
                        {
                            try
                            {
                                Log.Debug("Adding product {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                AddNewProduct(item);
                            }
                            catch (ProcessAbortedException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
								Log.Error("Product add error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                Log.Debug("Updating product {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateProductBalance(item, existingProd.First());
                                Log.Debug("Updating price {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateProductPriceAndActivity(item, existingProd.First());
                                Log.Debug("Updating discount info {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateDiscountInfo(item, existingProd.First());
                            }
                            catch (ProcessAbortedException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
								Log.Error("Product update error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }
                        break;
                    case GeneratedPriceType.SameItems:
                        if (existingProd == null || !existingProd.Any())
                        {
                            Log.Warn("Product does't exists. It will be added later. Reference: {0}", item.Reference);
                        }
                        else
                        {
                            try
                            {
                                Log.Debug("Updating balance {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateProductBalance(item, existingProd.First());
                                Log.Debug("Updating price {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateProductPriceAndActivity(item, existingProd.First());
                                Log.Debug("Updating discount info {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.UpdateDiscountInfo(item, existingProd.First());
                            }
                            catch (ProcessAbortedException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
								Log.Error("Balance update error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }

                        break;
                    case GeneratedPriceType.DeletedItems:
                        if (existingProd != null && existingProd.Any())
                        {
                            try
                            {
                                Log.Debug("Disabling product {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                this.DisableProduct(existingProd.First());
                            }
                            catch (ProcessAbortedException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
								Log.Error("Disable product error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }
                        break;
                }
            }
        }

        private void UpdateProductBalance(PriceItem priceItem, product product)
        {
            var stock = this.GetStockValue(priceItem, product);
            if (stock.quantity != priceItem.Balance)
            {
                stock.quantity = priceItem.Balance;
                _apiFactory.StockFactory.Update(stock);
            }
        }

        private void UpdateProductPriceAndActivity(PriceItem priceItem, product product)
        {
            if (product.price != Convert.ToDecimal(priceItem.RetailPrice) || product.wholesale_price != Convert.ToDecimal(priceItem.WholesalePrice) 
                || product.active != Convert.ToInt32(priceItem.Active)
                || !SameMetaInfo(priceItem, product))
            {
                product.price = Convert.ToDecimal(priceItem.RetailPrice);
                product.wholesale_price = Convert.ToDecimal(priceItem.WholesalePrice);
                product.active = Convert.ToInt32(priceItem.Active);

                ProductsMapper.FillMetaInfo(priceItem, product);

                _apiFactory.ProductFactory.Update(product);
            }
        }

        private void UpdateDiscountInfo(PriceItem priceItem, product product)
        {
            if(product.on_sale != Convert.ToInt32(priceItem.OnSale))
            {
                var filter = new Dictionary<string, string> { { "id_product", Convert.ToString(product.id) } };
                var specialPriceRule = _apiFactory.SpecialPriceFactory.GetByFilter(filter, null, null).FirstOrDefault();
                if(specialPriceRule != null)
                {
                    if(!priceItem.OnSale && product.on_sale == 1)
                    {
                        // remove special price
                        _apiFactory.SpecialPriceFactory.Delete(specialPriceRule);
                    }
                    else
	                {
                        if(specialPriceRule.reduction != priceItem.DiscountValue)
                        {
                            specialPriceRule.reduction = priceItem.DiscountValue;
                            _apiFactory.SpecialPriceFactory.Update(specialPriceRule);
                        }
	                }
                }
                else
                {
                    specialPriceRule = new specific_price
                    {
                        id_product = product.id,
                        reduction = Convert.ToDecimal(priceItem.DiscountValue) / 100,
                        reduction_type = "percentage",
                        id_shop = 1,
                        id_cart = 0,
                        id_currency = 0,
                        id_country = 0,
                        id_group = 0,
                        id_customer = 0,
                        from_quantity = 1,
                        price = -1,
                    };
                    _apiFactory.SpecialPriceFactory.Add(specialPriceRule);
                }
                
                product.on_sale = Convert.ToInt32(priceItem.OnSale);
                _apiFactory.ProductFactory.Update(product);
            }
        }

        private bool SameMetaInfo(PriceItem priceItem, product product)
        {
            if (string.IsNullOrWhiteSpace(priceItem.Name))
                return true;

            if (product.meta_title == null || 
                !product.meta_title.Any() || 
                !product.meta_title[0].Value.Equals(priceItem.Name, StringComparison.OrdinalIgnoreCase))
                return false;
            
            if (product.meta_description == null ||
                !product.meta_description.Any() ||
                !product.meta_description[0].Value.Equals(string.Format("Купить {0} в Москве", priceItem.Name), StringComparison.OrdinalIgnoreCase))
                return false;
            
            if (product.meta_keywords == null ||
                !product.meta_keywords.Any())
                return false;
            
            var words = priceItem.Name.Split(new char[] { ' ' }).Where(s => s.Length > 3);
            if (words.Any())
            {
                foreach (var word in words)
                {
                    if (!product.meta_keywords.Exists(s => s.Value.Equals(word, StringComparison.OrdinalIgnoreCase)))
                        return false;
                }
            }

            return true;
        }

        private void AddNewProduct(PriceItem priceItem)
        {
            var product = ProductsMapper.Create(priceItem);

            var category = this.GetCategoryValue(priceItem);
            product = ProductsMapper.MapCategory(product, category);

            var supplier = _apiFactory.Suppliers.First(s => s.name.Equals(priceItem.SupplierName, StringComparison.CurrentCultureIgnoreCase));
            product = ProductsMapper.MapSupplier(product, supplier);

            var featureValue = this.GetFeatureValue(priceItem.Size, _apiFactory.SizeFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Color, _apiFactory.ColorFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Material, _apiFactory.MaterialFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Country, _apiFactory.CountryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Packing, _apiFactory.PackingFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Length, _apiFactory.LengthFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Diameter, _apiFactory.DiameterFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Battery, _apiFactory.BatteryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            var manufacturerValue = this.GetManufacturerValue(priceItem, product);
            product = ProductsMapper.MapManufacturer(product, manufacturerValue);

            product = ProductsMapper.FillMetaInfo(priceItem, product);

            // Добавление продукта
            product = _apiFactory.ProductFactory.Add(product);

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

			if(product.associations.images == null || !product.associations.images.Any())
            {
                Log.Fatal("Unable to load product photos. Adding new products aborted. Product reference: {0}", priceItem.Reference);
                _apiFactory.ProductFactory.Delete(product.id.Value);
				Log.Debug("Product deleted. Reference: {0}", priceItem.Reference);
                throw new ProcessAbortedException();
            }

			this.GetProductSupplierValue(priceItem, product, supplier);

            this.UpdateProductBalance(priceItem, product);
        }

        private manufacturer GetManufacturerValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Manufacturer } };
            var manufacturers = _apiFactory.ManufacturerFactory.GetByFilter(filter, null, null);

            if (manufacturers == null || !manufacturers.Any())
            {
                var manufacturer = new manufacturer
                {
                    name = priceItem.Manufacturer,
                    active = 1,
                };
                return _apiFactory.ManufacturerFactory.Add(manufacturer);
            }
            return manufacturers.First();
        }

        private product_supplier GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var supplier = _apiFactory.ProductSupplierFactory.GetByFilter(filter, null, null).FirstOrDefault();
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
                supplier = _apiFactory.ProductSupplierFactory.Add(supplier);
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
                    return _apiFactory.ImageFactory.AddProductImage(product.id.Value, bytes);
                }
                catch (Exception ex)
                {
                    Log.Error("Error while loading product image", ex);
                    return null;
                }
            }
        }

        private category GetCategoryValue(PriceItem priceItem)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Category } };

            return _apiFactory.CategoryFactory.GetByFilter(filter, null, null).First();
        }

        private product_feature_value GetFeatureValue(string value, product_feature feature)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var filter = new Dictionary<string, string> { { "id_feature", feature.id.Value.ToString(CultureInfo.InvariantCulture) }, { "value", value }, };

            var featureValues = _apiFactory.FeatureValuesFactory.GetByFilter(filter, null, null);
            product_feature_value featureValue;
            if (featureValues == null || !featureValues.Any())
            {
                featureValue = new product_feature_value
                {
                    id_feature = feature.id,
                    value = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, value) }
                };
                featureValue = _apiFactory.FeatureValuesFactory.Add(featureValue);
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

            var stock = _apiFactory.StockFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (stock == null)
            {
                stock = new stock_available
                {
                    id_product = product.id,
                    quantity = priceItem.Balance
                };
                stock = _apiFactory.StockFactory.AddList(new List<stock_available> { stock }).First();
            }

            return stock;
        }

        private void DisableProduct(product product)
        {
            product.active = 0;
            _apiFactory.ProductFactory.Update(product);
        }
    }
}

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

namespace Supplier2Presta.Service.Processors
{
    public class PriceWebServiceProcessor : IProcessor
    {
        private static ProductFactory productFactory;
        private static CategoryFactory categoryFactory;
        private static StockAvailableFactory stockFactory;
        private static ProductFeatureValueFactory featureValuesFactory;
        private static ManufacturerFactory manufacturerFactory;
        private static ImageFactory imageFactory;
        private static SupplierFactory supplierFactory;
        private static ProductSupplierFactory productSupplierFactory;

        private static product_feature sizeFeature;
        private static product_feature colorFeature;
        private static product_feature materialFeature;
        private static product_feature countryFeature;
        private static product_feature packingFeature;
        private static product_feature lengthFeature;
        private static product_feature diameterFeature;
        private static product_feature batteryFeature;
        
        private static List<supplier> suppliers;

        private readonly IDiffer differ;

        public PriceWebServiceProcessor(IDiffer differ)
        {
            this.differ = differ;
        }

        public event EventDelegates.ProcessEventDelegate OnProductProcessed;

        public Tuple<int, int, int> Process(Diff diff)
        {
            if (this.OnProductProcessed != null)
            {
                this.OnProductProcessed("Подключение к API", GeneratedPriceType.None);
            }

            this.InitFactories();
            
            this.ProcessDiff(diff.NewItems, GeneratedPriceType.NewItems);
            this.ProcessDiff(diff.SameItems, GeneratedPriceType.SameItems);
            this.ProcessDiff(diff.DeletedItems, GeneratedPriceType.DeletedItems);

            return Tuple.Create(diff.SameItems.Count, diff.NewItems.Count, diff.DeletedItems.Count);
        }

        private void InitFactories()
        {
            const string BaseUrl = "http://dirty-dreams.ru/api";
            const string Account = "4MFF2R1ZFA4DXSYLDSKP8MK7V7W83KWL";
            const string Password = "";
            productFactory = new ProductFactory(BaseUrl, Account, Password);
            categoryFactory = new CategoryFactory(BaseUrl, Account, Password);
            stockFactory = new StockAvailableFactory(BaseUrl, Account, Password);
            featureValuesFactory = new ProductFeatureValueFactory(BaseUrl, Account, Password);
            imageFactory = new ImageFactory(BaseUrl, Account, Password);
            productSupplierFactory = new ProductSupplierFactory(BaseUrl, Account, Password);
            supplierFactory = new SupplierFactory(BaseUrl, Account, Password);
            suppliers = supplierFactory.GetAll();

            manufacturerFactory = new ManufacturerFactory(BaseUrl, Account, Password);

            var featuresFactory = new ProductFeatureFactory(BaseUrl, Account, Password);
            var features = featuresFactory.GetAll();

            sizeFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Размер", StringComparison.InvariantCultureIgnoreCase));
            if (sizeFeature == null)
            {
                sizeFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Размер") }
                };
                sizeFeature = featuresFactory.Add(sizeFeature);
            }

            colorFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Цвет", StringComparison.InvariantCultureIgnoreCase));
            if (colorFeature == null)
            {
                colorFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Цвет") }
                };
                colorFeature = featuresFactory.Add(colorFeature);
            }

            batteryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Батарейки", StringComparison.InvariantCultureIgnoreCase));
            if (batteryFeature == null)
            {
                batteryFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Батарейки") }
                };
                batteryFeature = featuresFactory.Add(batteryFeature);
            }

            materialFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Материал", StringComparison.InvariantCultureIgnoreCase));
            if (materialFeature == null)
            {
                materialFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Материал") }
                };
                materialFeature = featuresFactory.Add(materialFeature);
            }

            countryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Страна", StringComparison.InvariantCultureIgnoreCase));
            if (countryFeature == null)
            {
                countryFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Страна") }
                };
                countryFeature = featuresFactory.Add(countryFeature);
            }

            packingFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Упаковка", StringComparison.InvariantCultureIgnoreCase));
            if (packingFeature == null)
            {
                packingFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Упаковка") }
                };
                packingFeature = featuresFactory.Add(packingFeature);
            }

            lengthFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Длина", StringComparison.InvariantCultureIgnoreCase));
            if (lengthFeature == null)
            {
                lengthFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Длина") }
                };
                lengthFeature = featuresFactory.Add(lengthFeature);
            }

            diameterFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Диаметр", StringComparison.InvariantCultureIgnoreCase));
            if (diameterFeature == null)
            {
                diameterFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Диаметр") }
                };
                diameterFeature = featuresFactory.Add(diameterFeature);
            }
        }

        private void ProcessDiff(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType)
        {
            foreach (var item in priceItems.Values)
            {
                var filter = new Dictionary<string, string> { { "reference", item.Reference } };
                var existingProd = productFactory.GetByFilter(filter, null, null);

                switch (generatedPriceType)
                {
                    case GeneratedPriceType.NewItems:
                    case GeneratedPriceType.SameItems:
                        if (existingProd == null || !existingProd.Any())
                        {
                            this.AddNewProduct(item);
                        }
                        else
                        {
                            this.UpdateProductBalance(item, existingProd.First());
                            this.UpdateProductPrice(item, existingProd.First());
                        }

                        break;
                    case GeneratedPriceType.DeletedItems:
                        if (existingProd != null && existingProd.Any())
                        {
                            this.DisableProduct(existingProd.First());
                        }

                        break;
                }

                if (this.OnProductProcessed != null)
                {
                    this.OnProductProcessed(item.Name, generatedPriceType);
                }
            }
        }

        private void UpdateProductBalance(PriceItem priceItem, product product)
        {
            var stock = this.GetStockValue(priceItem, product);
            if (stock.quantity != priceItem.Balance)
            {
                stock.quantity = priceItem.Balance;
                stockFactory.Update(stock);
            }
        }

        private void UpdateProductPrice(PriceItem priceItem, product product)
        {
            if (product.price != Convert.ToDecimal(priceItem.RetailPrice) || product.wholesale_price != Convert.ToDecimal(priceItem.WholesalePrice))
            {
                product.price = Convert.ToDecimal(priceItem.RetailPrice);
                product.wholesale_price = Convert.ToDecimal(priceItem.WholesalePrice);
                productFactory.Update(product);
            }
        }

        private void AddNewProduct(PriceItem priceItem)
        {
            var product = ProductsMapper.Create(priceItem);

            var category = this.GetCategoryValue(priceItem);
            product = ProductsMapper.MapCategory(product, category);

            var supplier = suppliers.First(s => s.name.Equals(priceItem.SupplierName, StringComparison.CurrentCultureIgnoreCase));
            product = ProductsMapper.MapSupplier(product, supplier);

            var featureValue = this.GetFeatureValue(priceItem.Size, sizeFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Color, colorFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Material, materialFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Country, countryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Packing, packingFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Length, lengthFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Diameter, diameterFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = this.GetFeatureValue(priceItem.Battery, batteryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            var manufacturerValue = this.GetManufacturerValue(priceItem, product);
            product = ProductsMapper.MapManufacturer(product, manufacturerValue);

            // Добавление продукта
            product = productFactory.Add(product);
            
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
            var manufacturers = manufacturerFactory.GetByFilter(filter, null, null);

            if (manufacturers == null || !manufacturers.Any())
            {
                var manufacturer = new manufacturer
                {
                    name = priceItem.Manufacturer,
                    active = 1,
                };
                return manufacturerFactory.Add(manufacturer);
            }
            return manufacturers.First();
        }

        private product_supplier GetProductSupplierValue(PriceItem priceItem, product product, supplier supplierFeature)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var supplier = productSupplierFactory.GetByFilter(filter, null, null).FirstOrDefault();
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
                supplier = productSupplierFactory.Add(supplier);
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
                    return imageFactory.AddProductImage(product.id.Value, bytes);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private stock_available GetStockValue(PriceItem priceItem, product product)
        {
            var filter = new Dictionary<string, string>
            {
                { "id_product", product.id.Value.ToString(CultureInfo.InvariantCulture) }
            };

            var stock = stockFactory.GetByFilter(filter, null, null).FirstOrDefault();
            if (stock == null)
            {
                stock = new stock_available
                {
                    id_product = product.id,
                    quantity = priceItem.Balance
                };
                stock = stockFactory.AddList(new List<stock_available> { stock }).First();
            }

            return stock;
        }

        private category GetCategoryValue(PriceItem priceItem)
        {
            var filter = new Dictionary<string, string> { { "name", priceItem.Category } };

            return categoryFactory.GetByFilter(filter, null, null).First();
        }

        private product_feature_value GetFeatureValue(string value, product_feature feature)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var filter = new Dictionary<string, string> { { "id_feature", feature.id.Value.ToString(CultureInfo.InvariantCulture) }, { "value", value }, };

            var featureValues = featureValuesFactory.GetByFilter(filter, null, null);
            product_feature_value featureValue;
            if (featureValues == null || !featureValues.Any())
            {
                featureValue = new product_feature_value
                {
                    id_feature = feature.id,
                    value = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, value) }
                };
                featureValue = featureValuesFactory.Add(featureValue);
            }
            else
            {
                featureValue = featureValues.First();
            }

            return featureValue;
        }

        private void DisableProduct(product product)
        {
            product.active = 0;
            productFactory.Update(product);
        }
    }
}

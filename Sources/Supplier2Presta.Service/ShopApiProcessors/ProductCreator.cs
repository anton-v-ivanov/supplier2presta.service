using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.Exceptions;
using Supplier2Presta.Service.Helpers;
using Supplier2Presta.Service.ShopApiProcessors.EntityProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supplier2Presta.Service.ShopApiProcessors
{
    public class ProductCreator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly ShopApiFactory _apiFactory;
        private readonly CombinationsProcessor _combinationsProcessor;
        private readonly CategoryProcessor _categoryProcessor;
        private readonly FeatureProcessor _featureProcessor;
        private readonly ManufacturerProcessor _manufacturerProcessor;
        private readonly ImageProcessor _imageProcessor;
        private readonly SupplierProcessor _supplierProcessor;
        private readonly StockProcessor _stockProcessor;

        public ProductCreator(ShopApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
            _categoryProcessor = new CategoryProcessor(apiFactory);
            _combinationsProcessor = new CombinationsProcessor(apiFactory);
            _featureProcessor = new FeatureProcessor(apiFactory);
            _manufacturerProcessor = new ManufacturerProcessor(apiFactory);
            _supplierProcessor = new SupplierProcessor(apiFactory);
            _imageProcessor = new ImageProcessor(apiFactory);
            _stockProcessor = new StockProcessor(apiFactory);
        }

        public void Create(PriceItem priceItem)
        {
            var product = ProductsMapper.Create(priceItem);

            var category = _categoryProcessor.GetCategoryValue(priceItem);
            product = ProductsMapper.MapCategory(product, category);

            var supplier = _apiFactory.Suppliers.First(s => s.name.Equals(priceItem.SupplierName, StringComparison.CurrentCultureIgnoreCase));
            product = ProductsMapper.MapSupplier(product, supplier);

            var featureValue = _featureProcessor.GetFeatureValue(priceItem.Material, _apiFactory.MaterialFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = _featureProcessor.GetFeatureValue(priceItem.Country, _apiFactory.CountryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = _featureProcessor.GetFeatureValue(priceItem.Packing, _apiFactory.PackingFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = _featureProcessor.GetFeatureValue(priceItem.Length, _apiFactory.LengthFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = _featureProcessor.GetFeatureValue(priceItem.Diameter, _apiFactory.DiameterFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = _featureProcessor.GetFeatureValue(priceItem.Battery, _apiFactory.BatteryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            var manufacturerValue = _manufacturerProcessor.GetManufacturerValue(priceItem, product);
            product = ProductsMapper.MapManufacturer(product, manufacturerValue);

            product = ProductsMapper.FillMetaInfo(priceItem, product);

            // Добавление продукта
            product = _apiFactory.ProductFactory.Add(product);

            if (priceItem.Photos == null || !priceItem.Photos.Any())
            {
                var image1 = _imageProcessor.GetImageValue(priceItem.PhotoSmall, product);
                product = ProductsMapper.MapImage(product, image1);
            }

            foreach (var photo in priceItem.Photos)
            {
                var image = _imageProcessor.GetImageValue(photo, product);
                product = ProductsMapper.MapImage(product, image);
            }

            if (product.associations.images == null || !product.associations.images.Any())
            {
                Log.Error("Unable to load product photos. Product will be deleted. Product reference: {0}", priceItem.Reference);
                _apiFactory.ProductFactory.Delete(product.id.Value);
                Log.Debug("Product deleted. Reference: {0}", priceItem.Reference);
                throw new PhotoLoadException();
            }

            _supplierProcessor.GetProductSupplierValue(priceItem, product, supplier);

            _combinationsProcessor.FillOptions(priceItem, product);

            _stockProcessor.UpdateStockValue(priceItem, product);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Serilog;
using Supplier2Presta.Entities;
using Supplier2Presta.Entities.Exceptions;
using Supplier2Presta.Helpers;
using Supplier2Presta.ShopApiProcessors.EntityProcessors;

namespace Supplier2Presta.ShopApiProcessors
{
    public class ProductCreator: IProductCreator
    {
        private readonly IShopApiFactory _apiFactory;
        private readonly ICombinationsProcessor _combinationsProcessor;
        private readonly ICategoryProcessor _categoryProcessor;
        private readonly IFeatureProcessor _featureProcessor;
        private readonly IManufacturerProcessor _manufacturerProcessor;
        private readonly IImageProcessor _imageProcessor;
        private readonly ISupplierProcessor _supplierProcessor;
        private readonly IStockProcessor _stockProcessor;

        public ProductCreator(IShopApiFactory apiFactory, 
            ICombinationsProcessor combinationsProcessor, 
            ICategoryProcessor categoryProcessor, 
            IFeatureProcessor featureProcessor, 
            IManufacturerProcessor manufacturerProcessor, 
            IImageProcessor imageProcessor, 
            ISupplierProcessor supplierProcessor, 
            IStockProcessor stockProcessor)
        {
            _apiFactory = apiFactory;
            _combinationsProcessor = combinationsProcessor;
            _categoryProcessor = categoryProcessor;
            _featureProcessor = featureProcessor;
            _manufacturerProcessor = manufacturerProcessor;
            _imageProcessor = imageProcessor;
            _supplierProcessor = supplierProcessor;
            _stockProcessor = stockProcessor;
        }

        public async Task Create(PriceItem priceItem)
        {
            if (string.IsNullOrWhiteSpace(priceItem.PhotoSmall) && (priceItem.Photos == null || !priceItem.Photos.Any()))
            {
                return;
            }

			Log.Information("Adding product {0}", priceItem.Reference);

			var product = ProductsMapper.Create(priceItem);

			var categories = await _categoryProcessor.GetCategories(priceItem);
            product = ProductsMapper.MapCategories(product, categories);

            var supplier = _apiFactory.Suppliers.First(s => s.name.Equals(priceItem.SupplierName, StringComparison.CurrentCultureIgnoreCase));
            product = ProductsMapper.MapSupplier(product, supplier);

            var featureValue = await _featureProcessor.GetFeatureValue(priceItem.Material, _apiFactory.MaterialFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = await _featureProcessor.GetFeatureValue(priceItem.Country, _apiFactory.CountryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = await _featureProcessor.GetFeatureValue(priceItem.Packing, _apiFactory.PackingFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = await _featureProcessor.GetFeatureValue(priceItem.Length, _apiFactory.LengthFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = await _featureProcessor.GetFeatureValue(priceItem.Diameter, _apiFactory.DiameterFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            featureValue = await _featureProcessor.GetFeatureValue(priceItem.Battery, _apiFactory.BatteryFeature);
            product = ProductsMapper.MapFeature(product, featureValue);

            var manufacturerValue = await _manufacturerProcessor.GetManufacturerValue(priceItem, product);
            product = ProductsMapper.MapManufacturer(product, manufacturerValue);

            product = ProductsMapper.FillMetaInfo(priceItem, product);

            // Добавление продукта
            product = await _apiFactory.ProductFactory.Add(product);

            if (priceItem.Photos == null || !priceItem.Photos.Any())
            {
                var image = await _imageProcessor.GetImageValue(priceItem.PhotoSmall, product);
                product = ProductsMapper.MapImage(product, image);
                if (product.id_default_image == null)
                {
                    Log.Information("Setting default image for product {reference}. ImageId: {imageId}", priceItem.Reference, image.id);
                    product.id_default_image = image.id;
                    await _apiFactory.ProductFactory.Update(product);
                }
            }
            else
            {
                image coverPhoto = null;
                foreach (var photo in priceItem.Photos)
                {
                    var image = await _imageProcessor.GetImageValue(photo, product);
                    if (coverPhoto == null)
                        coverPhoto = image;

                    product = ProductsMapper.MapImage(product, image);
                }
                if (product.id_default_image == null && coverPhoto != null)
                {
                    Log.Information("Setting default image for product {reference}. ImageId: {imageId}", priceItem.Reference, coverPhoto.id);
                    product.id_default_image = coverPhoto.id;
                    await _apiFactory.ProductFactory.Update(product);
                }
            }

            if (product.associations.images == null || !product.associations.images.Any())
            {
                Log.Warning("Unable to load product photos. Product will be deleted. Product reference: {0}", priceItem.Reference);
                await _apiFactory.ProductFactory.Delete(product.id.Value);
                Log.Debug("Product deleted. Reference: {0}", priceItem.Reference);
                throw new PhotoLoadException();
            }

            await _supplierProcessor.GetProductSupplierValue(priceItem, product, supplier);

            await _combinationsProcessor.FillOptions(priceItem, product);

            await _stockProcessor.UpdateStockValue(priceItem, product);
        }
    }
}

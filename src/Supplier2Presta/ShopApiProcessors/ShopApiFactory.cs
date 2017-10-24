using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;
using Serilog;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;

namespace Supplier2Presta.ShopApiProcessors
{
    public class ShopApiFactory : IShopApiFactory
    {
        public StockAvailableFactory StockFactory { get; private set; }
        public CombinationFactory CombinationFactory { get; private set; }
        public ProductFactory ProductFactory { get; private set; }
        public CategoryFactory CategoryFactory { get; private set; }
        public ProductFeatureValueFactory FeatureValuesFactory { get; private set; }
        public ImageFactory ImageFactory { get; private set; }
        public ManufacturerFactory ManufacturerFactory { get; private set; }
        public ProductSupplierFactory ProductSupplierFactory { get; private set; }
        public SpecificPriceFactory SpecialPriceFactory { get; private set; }
        public ProductOptionValueFactory OptionsValueFactory { get; private set; }
        public SupplierFactory SupplierFactory { get; private set; }

        public product_option ColorOption { get; private set; }
        public product_option SizeOption { get; private set; }
        public product_feature MaterialFeature { get; private set; }
        public product_feature CountryFeature { get; private set; }
        public product_feature PackingFeature { get; private set; }
        public product_feature LengthFeature { get; private set; }
        public product_feature DiameterFeature { get; private set; }
        public product_feature BatteryFeature { get; private set; }

        public List<supplier> Suppliers { get; private set; }

        public async Task InitFactories(string url, string account)
        {
            string baseUrl = url;
            const string password = "";
            ProductFactory = new ProductFactory(baseUrl, account, password);
            CategoryFactory = new CategoryFactory(baseUrl, account, password);
            StockFactory = new StockAvailableFactory(baseUrl, account, password);
            FeatureValuesFactory = new ProductFeatureValueFactory(baseUrl, account, password);
            ImageFactory = new ImageFactory(baseUrl, account, password);
            ProductSupplierFactory = new ProductSupplierFactory(baseUrl, account, password);
            
            ManufacturerFactory = new ManufacturerFactory(baseUrl, account, password);

            SpecialPriceFactory = new SpecificPriceFactory(baseUrl, account, password);

            var featuresFactory = new ProductFeatureFactory(baseUrl, account, password);
            var features = await featuresFactory.GetAll();
            
            //SizeFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Размер", StringComparison.OrdinalIgnoreCase));
            //ColorFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Цвет", StringComparison.OrdinalIgnoreCase));

            BatteryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Батарейки", StringComparison.OrdinalIgnoreCase));
            if (BatteryFeature == null)
            {
                BatteryFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Батарейки") }
                };
                BatteryFeature = await featuresFactory.Add(BatteryFeature);
            }

            MaterialFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Материал", StringComparison.OrdinalIgnoreCase));
            if (MaterialFeature == null)
            {
                MaterialFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Материал") }
                };
                MaterialFeature = await featuresFactory.Add(MaterialFeature);
            }

            CountryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Страна", StringComparison.OrdinalIgnoreCase));
            if (CountryFeature == null)
            {
                CountryFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Страна") }
                };
                CountryFeature = await featuresFactory.Add(CountryFeature);
            }

            PackingFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Упаковка", StringComparison.OrdinalIgnoreCase));
            if (PackingFeature == null)
            {
                PackingFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Упаковка") }
                };
                PackingFeature = await featuresFactory.Add(PackingFeature);
            }

            LengthFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Длина", StringComparison.OrdinalIgnoreCase));
            if (LengthFeature == null)
            {
                LengthFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Длина") }
                };
                LengthFeature = await featuresFactory.Add(LengthFeature);
            }

            DiameterFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Диаметр", StringComparison.OrdinalIgnoreCase));
            if (DiameterFeature == null)
            {
                DiameterFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Диаметр") }
                };
                DiameterFeature = await featuresFactory.Add(DiameterFeature);
            }

            var optionsFactory = new ProductOptionFactory(baseUrl, account, password);
            var options = await optionsFactory.GetAll();

            SizeOption = options.FirstOrDefault(f => f.name.First().Value.Equals("size", StringComparison.OrdinalIgnoreCase));
            if (SizeOption == null)
            {
                Log.Error("Size option not found, add size option!");
                throw new Exception("Size option not found");
            }

            ColorOption = options.FirstOrDefault(f => f.name.First().Value.Equals("color", StringComparison.OrdinalIgnoreCase));
            if (ColorOption == null)
            {
                Log.Error("Color option not found, add size option!");
                throw new Exception("Color option not found");
            }

            OptionsValueFactory = new ProductOptionValueFactory(baseUrl, account, password);

            CombinationFactory = new CombinationFactory(baseUrl, account, password);

            SupplierFactory = new SupplierFactory(baseUrl, account, password);
            Suppliers = await SupplierFactory.GetAll();
            if(!Suppliers.Any())
                throw new Exception("Suppliers not found, add them manualy");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;
using language = Bukimedia.PrestaSharp.Entities.AuxEntities.language;

namespace Supplier2Presta.Service.ShopApiProcessors
{
    public class ShopApiFactory
    {
        public ProductFactory ProductFactory;
        public CategoryFactory CategoryFactory;
        public StockAvailableFactory StockFactory;
        public ProductFeatureValueFactory FeatureValuesFactory;
        public ManufacturerFactory ManufacturerFactory;
        public ImageFactory ImageFactory;
        public SupplierFactory SupplierFactory;
        public ProductSupplierFactory ProductSupplierFactory;
        public SpecificPriceFactory SpecialPriceFactory;

        public product_feature MaterialFeature;
        public product_feature CountryFeature;
        public product_feature PackingFeature;
        public product_feature LengthFeature;
        public product_feature DiameterFeature;
        public product_feature BatteryFeature;
        public product_feature SizeFeature;
        public product_feature ColorFeature;

        public product_option SizeOption;
        public product_option ColorOption;
        public ProductOptionValueFactory OptionsValueFactory;
        public CombinationFactory CombinationFactory;

        public List<supplier> Suppliers;
        
        public void InitFactories(string url, string account)
        {
            string baseUrl = url;
            const string password = "";
            ProductFactory = new ProductFactory(baseUrl, account, password);
            CategoryFactory = new CategoryFactory(baseUrl, account, password);
            StockFactory = new StockAvailableFactory(baseUrl, account, password);
            FeatureValuesFactory = new ProductFeatureValueFactory(baseUrl, account, password);
            ImageFactory = new ImageFactory(baseUrl, account, password);
            ProductSupplierFactory = new ProductSupplierFactory(baseUrl, account, password);
            
            SupplierFactory = new SupplierFactory(baseUrl, account, password);
            Suppliers = SupplierFactory.GetAll();

            ManufacturerFactory = new ManufacturerFactory(baseUrl, account, password);

            SpecialPriceFactory = new SpecificPriceFactory(baseUrl, account, password);

            var featuresFactory = new ProductFeatureFactory(baseUrl, account, password);
            var features = featuresFactory.GetAll();
            
            //SizeFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Размер", StringComparison.InvariantCultureIgnoreCase));
            //ColorFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Цвет", StringComparison.InvariantCultureIgnoreCase));

            BatteryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Батарейки", StringComparison.InvariantCultureIgnoreCase));
            if (BatteryFeature == null)
            {
                BatteryFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Батарейки") }
                };
                BatteryFeature = featuresFactory.Add(BatteryFeature);
            }

            MaterialFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Материал", StringComparison.InvariantCultureIgnoreCase));
            if (MaterialFeature == null)
            {
                MaterialFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Материал") }
                };
                MaterialFeature = featuresFactory.Add(MaterialFeature);
            }

            CountryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Страна", StringComparison.InvariantCultureIgnoreCase));
            if (CountryFeature == null)
            {
                CountryFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Страна") }
                };
                CountryFeature = featuresFactory.Add(CountryFeature);
            }

            PackingFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Упаковка", StringComparison.InvariantCultureIgnoreCase));
            if (PackingFeature == null)
            {
                PackingFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Упаковка") }
                };
                PackingFeature = featuresFactory.Add(PackingFeature);
            }

            LengthFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Длина", StringComparison.InvariantCultureIgnoreCase));
            if (LengthFeature == null)
            {
                LengthFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Длина") }
                };
                LengthFeature = featuresFactory.Add(LengthFeature);
            }

            DiameterFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Диаметр", StringComparison.InvariantCultureIgnoreCase));
            if (DiameterFeature == null)
            {
                DiameterFeature = new product_feature()
                {
                    name = new List<language> { new language(1, "Диаметр") }
                };
                DiameterFeature = featuresFactory.Add(DiameterFeature);
            }

            var optionsFactory = new ProductOptionFactory(baseUrl, account, password);
            var options = optionsFactory.GetAll();

            SizeOption = options.First(f => f.name.First().Value.Equals("size", StringComparison.InvariantCultureIgnoreCase));
            ColorOption = options.First(f => f.name.First().Value.Equals("color", StringComparison.InvariantCultureIgnoreCase));

            OptionsValueFactory = new ProductOptionValueFactory(baseUrl, account, password);

            CombinationFactory = new CombinationFactory(baseUrl, account, password);
        }
    }
}

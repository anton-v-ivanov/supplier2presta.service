using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string BaseUrl = url;
            string Account = account;
            const string Password = "";
            ProductFactory = new ProductFactory(BaseUrl, Account, Password);
            CategoryFactory = new CategoryFactory(BaseUrl, Account, Password);
            StockFactory = new StockAvailableFactory(BaseUrl, Account, Password);
            FeatureValuesFactory = new ProductFeatureValueFactory(BaseUrl, Account, Password);
            ImageFactory = new ImageFactory(BaseUrl, Account, Password);
            ProductSupplierFactory = new ProductSupplierFactory(BaseUrl, Account, Password);
            
            SupplierFactory = new SupplierFactory(BaseUrl, Account, Password);
            Suppliers = SupplierFactory.GetAll();

            ManufacturerFactory = new ManufacturerFactory(BaseUrl, Account, Password);

            SpecialPriceFactory = new SpecificPriceFactory(BaseUrl, Account, Password);

            var featuresFactory = new ProductFeatureFactory(BaseUrl, Account, Password);
            var features = featuresFactory.GetAll();
            
            //SizeFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Размер", StringComparison.InvariantCultureIgnoreCase));
            //ColorFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Цвет", StringComparison.InvariantCultureIgnoreCase));

            BatteryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Батарейки", StringComparison.InvariantCultureIgnoreCase));
            if (BatteryFeature == null)
            {
                BatteryFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Батарейки") }
                };
                BatteryFeature = featuresFactory.Add(BatteryFeature);
            }

            MaterialFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Материал", StringComparison.InvariantCultureIgnoreCase));
            if (MaterialFeature == null)
            {
                MaterialFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Материал") }
                };
                MaterialFeature = featuresFactory.Add(MaterialFeature);
            }

            CountryFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Страна", StringComparison.InvariantCultureIgnoreCase));
            if (CountryFeature == null)
            {
                CountryFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Страна") }
                };
                CountryFeature = featuresFactory.Add(CountryFeature);
            }

            PackingFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Упаковка", StringComparison.InvariantCultureIgnoreCase));
            if (PackingFeature == null)
            {
                PackingFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Упаковка") }
                };
                PackingFeature = featuresFactory.Add(PackingFeature);
            }

            LengthFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Длина", StringComparison.InvariantCultureIgnoreCase));
            if (LengthFeature == null)
            {
                LengthFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Длина") }
                };
                LengthFeature = featuresFactory.Add(LengthFeature);
            }

            DiameterFeature = features.FirstOrDefault(f => f.name.First().Value.Equals("Диаметр", StringComparison.InvariantCultureIgnoreCase));
            if (DiameterFeature == null)
            {
                DiameterFeature = new product_feature()
                {
                    name = new List<Bukimedia.PrestaSharp.Entities.AuxEntities.language> { new Bukimedia.PrestaSharp.Entities.AuxEntities.language(1, "Диаметр") }
                };
                DiameterFeature = featuresFactory.Add(DiameterFeature);
            }

            var optionsFactory = new ProductOptionFactory(BaseUrl, Account, Password);
            var options = optionsFactory.GetAll();

            SizeOption = options.First(f => f.name.First().Value.Equals("size", StringComparison.InvariantCultureIgnoreCase));
            ColorOption = options.First(f => f.name.First().Value.Equals("color", StringComparison.InvariantCultureIgnoreCase));

            OptionsValueFactory = new ProductOptionValueFactory(BaseUrl, Account, Password);

            CombinationFactory = new CombinationFactory(BaseUrl, Account, Password);
        }
    }
}

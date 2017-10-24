using System.Collections.Generic;
using System.Threading.Tasks;
using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;

namespace Supplier2Presta.ShopApiProcessors
{
    public interface IShopApiFactory
    {
        Task InitFactories(string url, string account);
        StockAvailableFactory StockFactory { get; }
        CombinationFactory CombinationFactory { get; }
        ProductFactory ProductFactory { get; }
        CategoryFactory CategoryFactory { get; }
        ProductFeatureValueFactory FeatureValuesFactory { get; }
        ImageFactory ImageFactory { get; }
        ManufacturerFactory ManufacturerFactory { get; }
        ProductSupplierFactory ProductSupplierFactory { get; }
        SpecificPriceFactory SpecialPriceFactory { get; }
        ProductOptionValueFactory OptionsValueFactory { get; }
        SupplierFactory SupplierFactory { get; }

        product_option ColorOption { get; }
        product_option SizeOption { get; }
        product_feature MaterialFeature { get; }
        product_feature CountryFeature { get; }
        product_feature PackingFeature { get; }
        product_feature LengthFeature { get; }
        product_feature DiameterFeature { get; }
        product_feature BatteryFeature { get; }
        //product_feature SizeFeature { get; }
        //product_feature ColorFeature { get; }

        List<supplier> Suppliers { get; }
    }
}
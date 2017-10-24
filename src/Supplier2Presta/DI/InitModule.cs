using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.Diffs;
using Supplier2Presta.Managers;
using Supplier2Presta.PriceItemBuilders;
using Supplier2Presta.ShopApiProcessors;
using Supplier2Presta.ShopApiProcessors.EntityProcessors;

namespace Supplier2Presta.DI
{
    public class InitModule: Module
    {
        private readonly IConfiguration _configuration;
        private readonly StartArgs _startArgs;

        public InitModule(IConfiguration configuration, StartArgs startArgs)
        {
            _configuration = configuration;
            _startArgs = startArgs;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var apiUrl = _configuration["api:url"];
            var apiAccessToken = _configuration["api:access_token"];

            var suppliers = new List<SupplierSettings>();
            _configuration.GetSection("suppliers").Bind(suppliers);

            var colorMappings = new List<ColorMapping>();
            _configuration.GetSection("color_mappings").Bind(colorMappings);

            var ignoredProducts = new List<string>();
            _configuration.GetSection("ignored_products").Bind(ignoredProducts);

            var robotConfig = new RobotConfiguration(apiUrl, apiAccessToken, suppliers, colorMappings, ignoredProducts);
            builder.RegisterInstance(robotConfig).As<IRobotConfiguration>();
            builder.RegisterType<ColorBuilder>().As<IColorBuilder>();
            builder.RegisterType<Differ>().As<IDiffer>();
            builder.RegisterType<PriceManagerBuilder>().As<IPriceManagerBuilder>();
            builder.RegisterType<PriceWebServiceProcessor>().As<IProcessor>();
            builder.RegisterInstance(_startArgs).AsSelf();

            Log.Debug("Connecting to API");
            var apiFactory = new ShopApiFactory();
            try
            {
                apiFactory.InitFactories(apiUrl, apiAccessToken).Wait();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Unable to connect to API");
                throw;
            }

            builder.RegisterInstance(apiFactory).As<IShopApiFactory>();

            builder.RegisterType<ProductCreator>().As<IProductCreator>();
            builder.RegisterType<ProductUpdater>().As<IProductUpdater>();
            builder.RegisterType<ProductRemover>().As<IProductRemover>();

            builder.RegisterType<CategoryProcessor>().As<ICategoryProcessor>();
            builder.RegisterType<CombinationsProcessor>().As<ICombinationsProcessor>();
            builder.RegisterType<FeatureProcessor>().As<IFeatureProcessor>();
            builder.RegisterType<ImageProcessor>().As<IImageProcessor>();
            builder.RegisterType<ManufacturerProcessor>().As<IManufacturerProcessor>();
            builder.RegisterType<StockProcessor>().As<IStockProcessor>();
            builder.RegisterType<SupplierProcessor>().As<ISupplierProcessor>();

            builder.RegisterType<Robot>().As<IRobot>();
        }
    }
}
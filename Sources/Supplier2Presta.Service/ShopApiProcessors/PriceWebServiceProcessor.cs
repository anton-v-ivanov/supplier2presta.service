using System;
using System.Linq;
using Bukimedia.PrestaSharp.Entities;
using NLog;
using Supplier2Presta.Service.Entities;
using System.Collections.Generic;
using Supplier2Presta.Service.Entities.Exceptions;

namespace Supplier2Presta.Service.ShopApiProcessors
{
    public class PriceWebServiceProcessor : IProcessor
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private readonly ShopApiFactory _apiFactory;
        private string _apiUrl;
        private string _accessToken;
        
        private ProductUpdater _productUpdater;
        private ProductRemover _productRemover;
        private ProductCreator _productCreator;

        public PriceWebServiceProcessor(string apiUrl, string accessToken)
        {
            _apiFactory = new ShopApiFactory();
            _apiUrl = apiUrl;
            _accessToken = accessToken;
        }

        public void Process(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType, PriceType processingPriceType)
        {
            Log.Debug("Connecting to API");

            _apiFactory.InitFactories(_apiUrl, _accessToken);
            _productUpdater = new ProductUpdater(_apiFactory);
            _productRemover = new ProductRemover(_apiFactory);
            _productCreator = new ProductCreator(_apiFactory);

            ProcessDiff(priceItems, generatedPriceType, processingPriceType);
        }
        
        private void ProcessDiff(Dictionary<string, PriceItem> priceItems, GeneratedPriceType generatedPriceType, PriceType processingPriceType)
        {
            int currentCount = 0;
            var photoLoadErrorsOccured = false;

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
                                _productCreator.Create(item);
                            }
                            catch (PhotoLoadException)
                            {
                                photoLoadErrorsOccured = true;
                            }
                            catch (Exception ex)
                            {
								Log.Error("Product add error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }
                        else
                        {
                            Log.Info("Price and balance will be updated by Stock file. Product {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                        }
                        break;
                    case GeneratedPriceType.SameItems:
                        if (processingPriceType != PriceType.Full)
                        {
                            if (existingProd == null || !existingProd.Any())
                            {
                                Log.Warn("Product does't exists. It will be added later. Reference: {0}", item.Reference);
                            }
                            else
                            {
                                try
                                {
                                    Log.Debug("Updating balance {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                    _productUpdater.Update(existingProd.First(), item, processingPriceType);
                                }
                                catch (Exception ex)
                                {
                                    Log.Error("Balance update error. Reference: {0}; {1}", item.Reference, ex);
                                }
                            }
                        }
                        break;
                    case GeneratedPriceType.DeletedItems:
                        if (existingProd != null && existingProd.Any())
                        {
                            try
                            {
                                Log.Debug("Disabling product {0} from {1}; Reference: {2}", currentCount, priceItems.Count, item.Reference);
                                _productRemover.Remove(existingProd.First());
                            }
                            catch (Exception ex)
                            {
								Log.Error("Disable product error. Reference: {0}; {1}", item.Reference, ex);
                            }
                        }
                        break;
                }
            }
            if(photoLoadErrorsOccured)
            {
                throw new PhotoLoadException();
            }
        }
    }
}

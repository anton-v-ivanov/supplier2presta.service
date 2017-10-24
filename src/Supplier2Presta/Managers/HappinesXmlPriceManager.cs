using System;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.Entities;
using Supplier2Presta.Entities.XmlPrice;
using Supplier2Presta.Loaders;
using Supplier2Presta.PriceBuilders;
using Supplier2Presta.PriceItemBuilders;
using Supplier2Presta.ShopApiProcessors;

namespace Supplier2Presta.Managers
{
	public class HappinesXmlPriceManager : PriceManagerBase
	{
		private readonly IColorBuilder _colorCodeBuilder;

		public HappinesXmlPriceManager(IProcessor processor, SupplierSettings supplierSettings, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, IColorBuilder colorCodeBuilder)
			: base(processor, supplierSettings, archiveDirectory, retailPriceBuilder)
		{
			_colorCodeBuilder = colorCodeBuilder;
		}

		public override LoadUpdatesResult LoadUpdates(PriceType type, bool forceUpdate)
		{
			var xmlPriceLoader = new XmlPriceLoader(_colorCodeBuilder);
			var oldPriceLoader = new NewestFileSystemPriceLoader(xmlPriceLoader);
			var newPriceLoader = new SingleFilePriceLoader(xmlPriceLoader);

			PriceLoadResult newPriceLoadResult;
			PriceLoadResult oldPriceLoadResult = null;
			switch (type)
			{
				case PriceType.Stock:
					Log.Debug("Loading new price from {0}", PriceUrl);
					newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(PriceUrl);
					if (!forceUpdate)
					{
						Log.Debug("Loading old price from {0}", ArchiveDirectory);
						oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(ArchiveDirectory);
					}
					break;

				case PriceType.Full:
					Log.Debug("Loading new price from {0}", PriceUrl);
					newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(PriceUrl);
					if (!forceUpdate)
					{
						oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(ArchiveDirectory);
						Log.Debug("Loading old price from {0}", ArchiveDirectory);
					}
					break;

				case PriceType.Discount:
					throw new NotImplementedException();

				default:
					throw new ArgumentOutOfRangeException();
			}

			return new LoadUpdatesResult(newPriceLoadResult, oldPriceLoadResult, newPriceLoadResult.Success);
		}
	}
}

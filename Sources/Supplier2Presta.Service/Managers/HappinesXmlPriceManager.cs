using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Entities.XmlPrice;
using Supplier2Presta.Service.Loaders;
using Supplier2Presta.Service.PriceBuilders;
using Supplier2Presta.Service.PriceItemBuilders;
using System;

namespace Supplier2Presta.Service.Managers
{
	public class HappinesXmlPriceManager : PriceManagerBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();
		private readonly IColorBuilder _colorCodeBuilder;

		public HappinesXmlPriceManager(string priceUrl, int discountValue, string archiveDirectory, IRetailPriceBuilder retailPriceBuilder, string apiUrl, string apiAccessToken, IColorBuilder colorCodeBuilder)
			: base(priceUrl, discountValue, archiveDirectory, retailPriceBuilder, apiUrl, apiAccessToken)
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
					Log.Debug("Loading new price from {0}", this.PriceUrl);
					newPriceLoadResult = newPriceLoader.Load<StockXmlItemList>(PriceUrl);
					if (!forceUpdate)
					{
						Log.Debug("Loading old price from {0}", this.ArchiveDirectory);
						oldPriceLoadResult = oldPriceLoader.Load<StockXmlItemList>(ArchiveDirectory);
					}
					break;

				case PriceType.Full:
					Log.Debug("Loading new price from {0}", this.PriceUrl);
					newPriceLoadResult = newPriceLoader.Load<FullXmlItemList>(PriceUrl);
					if (!forceUpdate)
					{
						oldPriceLoadResult = oldPriceLoader.Load<FullXmlItemList>(ArchiveDirectory);
						Log.Debug("Loading old price from {0}", this.ArchiveDirectory);
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

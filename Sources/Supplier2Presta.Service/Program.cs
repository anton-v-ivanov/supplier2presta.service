using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using NLog;
using Supplier2Presta.Service.Entities;
using Supplier2Presta.Service.Config;
using Supplier2Presta.Service.Diffs;
using Supplier2Presta.Service.Managers;
using Supplier2Presta.Service.PriceItemBuilders;

namespace Supplier2Presta.Service
{
	public class Program
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public static int Main(string[] args)
		{
			Log.Info("Price update started");

			var updateTypes = new List<PriceType>();
			List<string> debugReferences = null;

			var forceUpdate = false;
			if (args != null)
			{
				forceUpdate = args.Any(s => s.Equals("force", StringComparison.OrdinalIgnoreCase));
				var debugRefStr = args.FirstOrDefault(s => s.StartsWith("r:", StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrEmpty(debugRefStr))
					debugReferences = debugRefStr.Replace("r:", string.Empty).Split(',').ToList();

				var typeStr = args.FirstOrDefault(s => s.StartsWith("t:", StringComparison.OrdinalIgnoreCase));
				if (!string.IsNullOrWhiteSpace(typeStr))
				{
					var typesStr = typeStr.Replace("t:", string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var type in typesStr)
					{
						if (type.Equals("all", StringComparison.OrdinalIgnoreCase))
						{
							updateTypes.Clear();
							updateTypes.AddRange(Enum.GetValues(typeof(PriceType)).Cast<PriceType>());
							break;
						}

						var priceType = (PriceType)Enum.Parse(typeof(PriceType), type, true);
						if (!updateTypes.Contains(priceType))
							updateTypes.Add(priceType);
					}
				}
				else
				{
					updateTypes.Add(PriceType.Stock);
				}
			}

			Log.Info("Update price types: {0}", string.Join(",", updateTypes.ToArray()));

			var apiUrl = ConfigurationManager.AppSettings["api-url"];
			var apiAccessToken = ConfigurationManager.AppSettings["api-access-token"];
			var ignoredProducts = RobotSettings.Config.IgnoredProducts.Cast<IgnoredProductElement>().Select(s => s.Reference).ToList();

			var colorsDictionary = RobotSettings.Config.Colors.Cast<ColorMappingElement>()
				.ToDictionary(color => color.Name, color => color.Code);

			var colorCodeBuilder = new ColorBuilder(colorsDictionary);

			foreach (SupplierElement settings in RobotSettings.Config.Suppliers)
			{
				if (updateTypes.Contains(settings.PriceType))
				{
					var priceManager = PriceManagerBuilder.Build(settings, apiUrl, apiAccessToken, colorCodeBuilder);

					Log.Info("Processing price: '{0}' Type: {1}", settings.Name, settings.PriceType);

					var loadUpdatesResult = priceManager.LoadUpdates(settings.PriceType, forceUpdate);
					if (loadUpdatesResult.IsSuccess)
					{
						Log.Debug("Building the diff");
						var diff = new Differ().GetDiff(loadUpdatesResult.NewPriceLoadResult.PriceItems, loadUpdatesResult.OldPriceLoadResult?.PriceItems, ignoredProducts);
						if (debugReferences != null && debugReferences.Any())
						{
							diff.NewItems = diff.NewItems.Where(s => debugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
							diff.NewItems = diff.UpdatedItems.Where(s => debugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
							diff.NewItems = diff.DeletedItems.Where(s => debugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
						}

						var result = priceManager.Process(diff, settings.PriceType);

						Log.Info($"{loadUpdatesResult.NewPriceLoadResult.PriceItems.Count} lines processed. {diff.UpdatedItems.Count} updated, {diff.NewItems.Count} added, {diff.DeletedItems.Count} deleted");

						if (result.Status != PriceUpdateResultStatus.Ok)
						{
							Log.Info("Price update finished. ErrorCode: " + result.Status);
							File.Delete(loadUpdatesResult.NewPriceLoadResult.FilePath);
							return Convert.ToInt32(result.Status);
						}

						if (diff.DeletedItems.Any() || diff.NewItems.Any() || diff.UpdatedItems.Any())
						{
							var file = Path.GetFileName(loadUpdatesResult.NewPriceLoadResult.FilePath);
							File.Move(loadUpdatesResult.NewPriceLoadResult.FilePath, Path.Combine(settings.ArchiveDirectory, file));
						}
						else
						{
							File.Delete(loadUpdatesResult.NewPriceLoadResult.FilePath);
						}
					}
					else
					{
						Log.Fatal("Unable to load new price");
						return Convert.ToInt32(PriceUpdateResultStatus.PriceLoadFail);
					}
				}
			}

			Log.Info("Price update finished successfully");
			return Convert.ToInt32(PriceUpdateResultStatus.Ok);
		}
	}
}
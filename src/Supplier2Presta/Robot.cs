using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.Diffs;
using Supplier2Presta.Entities;
using Supplier2Presta.Managers;

namespace Supplier2Presta
{
    public class Robot : IRobot
    {
        private readonly IRobotConfiguration _configuration;
        private readonly StartArgs _startArgs;
        private readonly IPriceManagerBuilder _priceManagerBuilder;
        private readonly IDiffer _differ;

        public Robot(IRobotConfiguration configuration, StartArgs startArgs, IPriceManagerBuilder priceManagerBuilder, IDiffer differ)
        {
            _configuration = configuration;
            _startArgs = startArgs;
            _priceManagerBuilder = priceManagerBuilder;
            _differ = differ;
        }

        public async Task<PriceUpdateResultStatus> ProcessPrice(CancellationTokenSource cancellationToken)
        {
            foreach (var supplierSettings in _configuration.Suppliers)
            {
                if (cancellationToken.IsCancellationRequested)
                    return PriceUpdateResultStatus.ProcessAborted;

                if (!_startArgs.UpdateTypes.Contains(supplierSettings.Type))
                    continue;

                var priceManager = _priceManagerBuilder.Build(supplierSettings);

                Log.Information("Processing price: '{Name}' Type: {PriceType}", supplierSettings.Name, supplierSettings.Type);

                var loadUpdatesResult = priceManager.LoadUpdates(supplierSettings.Type, _startArgs.IsForce);
                if (loadUpdatesResult.IsSuccess)
                {
                    Log.Debug("Building the diff");
                    var diff = _differ.GetDiff(loadUpdatesResult.NewPriceLoadResult.PriceItems, loadUpdatesResult.OldPriceLoadResult?.PriceItems, _configuration.IgnoredProducts);
                    if (_startArgs.DebugReferences != null && _startArgs.DebugReferences.Any())
                    {
                        diff.NewItems = diff.NewItems.Where(s => _startArgs.DebugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
                        diff.UpdatedItems = diff.UpdatedItems.Where(s => _startArgs.DebugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
                        diff.DeletedItems = diff.DeletedItems.Where(s => _startArgs.DebugReferences.Contains(s.Key)).ToDictionary(k => k.Key, pair => pair.Value);
                    }

                    var result = await priceManager.Process(diff, supplierSettings.Type);

                    Log.Information("{total} lines processed. {updated} updated, {new} added, {deleted} deleted",
                        loadUpdatesResult.NewPriceLoadResult.PriceItems.Count, diff.UpdatedItems.Count, diff.NewItems.Count, diff.DeletedItems.Count);

                    if (result.Status != PriceUpdateResultStatus.Ok)
                    {
                        Log.Information("Price update finished. ErrorCode: " + result.Status);
                        File.Delete(loadUpdatesResult.NewPriceLoadResult.FilePath);
                        return result.Status;
                    }

                    if (diff.DeletedItems.Any() || diff.NewItems.Any() || diff.UpdatedItems.Any())
                    {
                        var file = Path.GetFileName(loadUpdatesResult.NewPriceLoadResult.FilePath);
                        var archivePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), supplierSettings.ArchiveDirectory);
                        File.Move(loadUpdatesResult.NewPriceLoadResult.FilePath, Path.Combine(archivePath, file));
                    }
                    else
                    {
                        File.Delete(loadUpdatesResult.NewPriceLoadResult.FilePath);
                    }
                }
                else
                {
                    Log.Fatal("Unable to load new price");
                    return PriceUpdateResultStatus.PriceLoadFail;
                }
            }

            Log.Information("Price update finished successfully");
            return PriceUpdateResultStatus.Ok;
        }
    }
}
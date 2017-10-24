using System;
using System.Threading;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using Supplier2Presta.Config;
using Supplier2Presta.DI;
using Supplier2Presta.Entities;

namespace Supplier2Presta
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Information("Price update started");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("./config.json", false)
                .AddJsonFile("./config.real.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

            var startArgs = StartArgs.FromArgs(args);

            var builder = new ContainerBuilder();
            var initModule = new InitModule(configuration, startArgs);
            builder.RegisterModule(initModule);

            var container = builder.Build();

            var robot = container.Resolve<IRobot>();

            var cancellationToken = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                cancellationToken.Cancel();
            };

            var result = PriceUpdateResultStatus.Ok;
            robot.ProcessPrice(cancellationToken)
                .ContinueWith(res => result = res.Result, cancellationToken.Token)
                .Wait(cancellationToken.Token);

            return Convert.ToInt32(result);
        }
    }
}
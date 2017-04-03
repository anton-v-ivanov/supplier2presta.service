using System;
using System.IO;
using System.Net;
using System.Reflection;
using NLog;

namespace Supplier2Presta.Service.Loaders
{
	public class SingleFilePriceLoader : IPriceLoader
	{
		private readonly IPriceLoader _internalLoader;
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public SingleFilePriceLoader(IPriceLoader priceLoader)
		{
			_internalLoader = priceLoader;
		}

		public PriceLoadResult Load<T>(string uri)
		{
			var ext = Path.GetExtension(uri);
			var newPriceFileName = $"price_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{ext}";
			newPriceFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), newPriceFileName);
			if (uri.StartsWith("http"))
			{
				using (var webClient = new WebClient())
				{
					try
					{
						webClient.DownloadFile(uri, newPriceFileName);
					}
					catch (WebException ex)
					{
						Log.Fatal(ex);
						return new PriceLoadResult(null, false);
					}
				}
			}
			else
			{
				File.Copy(uri, newPriceFileName);
			}

			return _internalLoader.Load<T>(newPriceFileName);
			}
		}
	}

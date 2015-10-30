using System;
using System.Net.Http;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public class HalHttpClientFactory : HalHttpClientFactoryBase, IHalHttpClientFactory
	{
		public HalHttpClientFactory(IHalJsonParser parser) : base(parser)
		{
		}

		protected virtual void Configure(IHalHttpClientConfiguration config)
		{
			// Do nothing by default ...
		}

		protected virtual IHalHttpClient Transform(IHalHttpClient original)
		{
			return original; // return original by default ...
		}

		public IHalHttpClient CreateClient()
		{
			return CreateClient(null);
		}

		public IHalHttpClient CreateClient(HttpClient customHttpClient)
		{
			var httpClient = customHttpClient ?? new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});
			var halHttpClient = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(halHttpClient);

				var transformed = Transform(halHttpClient);

				halHttpClient.CachedApiRootResource = GetApiRootResource(transformed, halHttpClient);

				return transformed;
			}
			catch (Exception)
			{
				halHttpClient.Dispose(); // client is unusable ...
				throw;
			}
		}
	}

	public class HalHttpClientFactory<T> : HalHttpClientFactoryBase, IHalHttpClientFactory<T>
	{
		public HalHttpClientFactory(IHalJsonParser parser) : base(parser)
		{
		}

		protected virtual void Configure(IHalHttpClientConfiguration config, T context)
		{
			// Do nothing by default ...
		}

		protected virtual IHalHttpClient Transform(IHalHttpClient original, T context)
		{
			return original; // return original by default ...
		}

		public IHalHttpClient CreateClient(T context)
		{
			return CreateClient(null, context);
		}

		public IHalHttpClient CreateClient(HttpClient customHttpClient, T context)
		{
			var httpClient = customHttpClient ?? new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});
			var halHttpClient = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(halHttpClient, context);

				var transformed = Transform(halHttpClient, context);

				halHttpClient.CachedApiRootResource = GetApiRootResource(transformed, halHttpClient);

				return transformed;
			}
			catch (Exception)
			{
				halHttpClient.Dispose(); // client is unusable ...
				throw;
			}
		}
	}
}
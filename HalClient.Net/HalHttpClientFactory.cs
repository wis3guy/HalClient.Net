using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public class HalHttpClientFactory : IHalHttpClientFactory
	{
		private IRootResourceObject _cachedApiRootResource;

		protected readonly IHalJsonParser Parser;

		public HalHttpClientFactory(IHalJsonParser parser)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			Parser = parser;
		}

		protected virtual void Configure(IHalHttpClientConfiguration config)
		{
			// Do nothing by default ...
		}

		protected virtual IHalHttpClient Transform(IHalHttpClient original)
		{
			return original; // return original by default ...
		}

		protected virtual HttpClient GetHttpClient()
		{
			return new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
		}

		protected virtual HttpClient GetHttpClient(HttpMessageHandler httpMessageHandler)
		{
			return new HttpClient(httpMessageHandler);
		}

		protected IRootResourceObject GetApiRootResource(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			if (config.ApiRootResourceCachingBehavior != CachingBehavior.Never)
			{
				switch (config.ApiRootResourceCachingBehavior)
				{
					case CachingBehavior.Once:
						return GetCachedRootResource(client, config);
					case CachingBehavior.PerClient:
						return GetFreshRootResource(client, config);
				}
			}

			return null;
		}

		private IRootResourceObject GetCachedRootResource(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			_cachedApiRootResource = _cachedApiRootResource ?? GetFreshRootResource(client, config);

			return _cachedApiRootResource;
		}

		private static IRootResourceObject GetFreshRootResource(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			if (config.BaseAddress == null)
				throw new InvalidOperationException("The root resource can only be requested for caching if the BaseAddress of the client is initialized in the Configure method of the factory.");

			var task = client.GetAsync(config.BaseAddress);
			task.ConfigureAwait(false);
			task.Wait();

			return task.Result;
		}

		public IHalHttpClient CreateClient()
		{
			return CreateClient(GetHttpClient());
		}

		public IHalHttpClient CreateClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

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

		public IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateClient(GetHttpClient(httpMessageHandler));
		}
	}

	public class HalHttpClientFactory<T> : HalHttpClientFactory, IHalHttpClientFactory<T>
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
			return CreateClient(GetHttpClient(), context);
		}

		public IHalHttpClient CreateClient(HttpClient httpClient, T context)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

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

		public IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler, T context)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateClient(GetHttpClient(httpMessageHandler), context);
		}
	}
}
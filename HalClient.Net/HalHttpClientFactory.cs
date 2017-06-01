using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public class HalHttpClientFactory : HalHttpClientFactoryBase, IHalHttpClientFactory
	{
		private IRootResourceObject _cachedApiRootResource;

		public HalHttpClientFactory(IHalJsonParser parser) : base(parser)
		{
		}

		public IHalHttpClient CreateClient()
		{
			return CreateHalHttpClient(GetHttpClient());
		}

		public IHalHttpClient CreateClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			return CreateHalHttpClient(httpClient);
		}

		public IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClient(GetHttpClient(httpMessageHandler));
		}

		public Task<IHalHttpClient> CreateClientAsync(CachingBehavior apiRootCachingBehavior)
		{
			return CreateHalHttpClientAsync(GetHttpClient(), apiRootCachingBehavior);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClientAsync(GetHttpClient(httpMessageHandler), apiRootCachingBehavior);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never)
		{
			return CreateHalHttpClientAsync(httpClient, apiRootCachingBehavior);
		}

		protected virtual void Configure(IHalHttpClientConfiguration config)
		{
			// Do nothing by default ...
		}

		protected virtual IHalHttpClient Decorate(IHalHttpClient original)
		{
			return original; // return original by default ...
		}

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			var wrapped = new HalHttpClient(HalJsonParser, httpClient);

			try
			{
				Configure(wrapped.Configuration);

				var decorated = Decorate(wrapped) ?? wrapped;

				switch (apiRootCachingBehavior)
				{
					case CachingBehavior.Never:
						break;
					case CachingBehavior.PerClient:
						var apiRootResource = await GetFreshRootResourceAsync(decorated, wrapped.Configuration).ConfigureAwait(false);
						wrapped.CachedApiRootResource = apiRootResource;
						break;
					case CachingBehavior.Once:
						_cachedApiRootResource = _cachedApiRootResource ?? await GetFreshRootResourceAsync(decorated, wrapped.Configuration).ConfigureAwait(false);
						wrapped.CachedApiRootResource = _cachedApiRootResource;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(apiRootCachingBehavior), apiRootCachingBehavior, null);
				}

				return decorated;
			}
			catch (Exception)
			{
				wrapped.Dispose(); // client is unusable ...
				throw;
			}
		}

		private IHalHttpClient CreateHalHttpClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			var wrapped = new HalHttpClient(HalJsonParser, httpClient);

			try
			{
				Configure(wrapped.Configuration);

				var decorated = Decorate(wrapped) ?? wrapped;

				return decorated;
			}
			catch (Exception)
			{
				wrapped.Dispose(); // client is unusable ...
				throw;
			}
		}
	}
}
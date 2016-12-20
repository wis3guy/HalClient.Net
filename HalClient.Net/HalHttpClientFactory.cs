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

		protected virtual IHalHttpClient Decorate(IHalHttpClient original)
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

		protected static async Task<IRootResourceObject> GetFreshRootResourceAsync(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			if (config.BaseAddress == null)
				throw new InvalidOperationException("The root resource can only be requested for caching if the BaseAddress of the client is initialized in the Configure method of the factory.");

			var message = await client.GetAsync(config.BaseAddress).ConfigureAwait(false);

			return message.Resource;
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

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			var wrapped = new HalHttpClient(Parser, httpClient);

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

			var wrapped = new HalHttpClient(Parser, httpClient);

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
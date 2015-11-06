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

			return await client.GetAsync(config.BaseAddress).ConfigureAwait(false);
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

		public Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, CachingBehavior apiRootCachingBehavior)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClientAsync(GetHttpClient(httpMessageHandler), apiRootCachingBehavior);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			return CreateHalHttpClientAsync(httpClient, apiRootCachingBehavior);
		}

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			var wrapped = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(wrapped);

				var decorated = Decorate(wrapped);

				switch (apiRootCachingBehavior)
				{
					case CachingBehavior.Never:
						break;
					case CachingBehavior.PerClient:
						var apiRootResource = await GetFreshRootResourceAsync(decorated, wrapped).ConfigureAwait(false);
						wrapped.CachedApiRootResource = apiRootResource;
						break;
					case CachingBehavior.Once:
						_cachedApiRootResource = _cachedApiRootResource ?? await GetFreshRootResourceAsync(decorated, wrapped).ConfigureAwait(false);
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

			var halHttpClient = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(halHttpClient);
				var decorated = Decorate(halHttpClient);
				return decorated;
			}
			catch (Exception)
			{
				halHttpClient.Dispose(); // client is unusable ...
				throw;
			}
		}
	}

	public abstract class HalHttpClientFactory<T> : HalHttpClientFactory, IHalHttpClientFactory<T>
	{
		private IRootResourceObject _cachedApiRootResource;

		protected HalHttpClientFactory(IHalJsonParser parser) : base(parser)
		{
		}

		protected abstract void Configure(IHalHttpClientConfiguration config, T context);

		protected abstract IHalHttpClient Decorate(IHalHttpClient original, T context);

		public IHalHttpClient CreateClient(T context)
		{
			return CreateHalHttpClient(GetHttpClient(), context);
		}

		public IHalHttpClient CreateClient(HttpClient httpClient, T context)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			return CreateHalHttpClient(httpClient, context);
		}

		public IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler, T context)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClient(GetHttpClient(httpMessageHandler), context);
		}

		public Task<IHalHttpClient> CreateClientAsync(CachingBehavior apiRootCachingBehavior, T context)
		{
			return CreateHalHttpClientAsync(GetHttpClient(), apiRootCachingBehavior, context);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior, T context)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			return CreateHalHttpClientAsync(httpClient, apiRootCachingBehavior, context);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, CachingBehavior apiRootCachingBehavior, T context)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClientAsync(GetHttpClient(httpMessageHandler), apiRootCachingBehavior, context);
		}

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior, T context)
		{
			var wrapped = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(wrapped, context);

				var decorated = Decorate(wrapped, context);

				switch (apiRootCachingBehavior)
				{
					case CachingBehavior.Never:
						break;
					case CachingBehavior.PerClient:
						var apiRootResource = await GetFreshRootResourceAsync(decorated, wrapped).ConfigureAwait(false);
						wrapped.CachedApiRootResource = apiRootResource;
						break;
					case CachingBehavior.Once:
						_cachedApiRootResource = _cachedApiRootResource ?? await GetFreshRootResourceAsync(decorated, wrapped).ConfigureAwait(false);
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

		private IHalHttpClient CreateHalHttpClient(HttpClient httpClient, T context, Action<HalHttpClient> preDecorate = null)
		{
			var halHttpClient = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(halHttpClient, context);
				preDecorate?.Invoke(halHttpClient);
				var decorated = Decorate(halHttpClient, context);
				return decorated;
			}
			catch (Exception)
			{
				halHttpClient.Dispose(); // client is unusable ...
				throw;
			}
		}
	}
}
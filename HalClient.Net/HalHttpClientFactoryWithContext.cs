using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public class HalHttpClientFactoryWithContext<T> : HalHttpClientFactoryBase, IHalHttpClientFactoryWithContext<T>
	{
		private IRootResourceObject _cachedApiRootResource;

		public HalHttpClientFactoryWithContext(IHalJsonParser parser) : base(parser)
		{
		}

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

		public Task<IHalHttpClient> CreateClientAsync(T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never)
		{
			return CreateHalHttpClientAsync(GetHttpClient(), context, apiRootCachingBehavior);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			return CreateHalHttpClientAsync(httpClient, context, apiRootCachingBehavior);
		}

		public Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClientAsync(GetHttpClient(httpMessageHandler), context, apiRootCachingBehavior);
		}

		protected virtual void Configure(IHalHttpClientConfiguration config, T context)
		{
			// Do nothing by default ...
		}

		protected virtual IHalHttpClient Decorate(IHalHttpClient original, T context)
		{
			return original; // return original by default ...
		}

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, T context, CachingBehavior apiRootCachingBehavior)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			var wrapped = new HalHttpClient(HalJsonParser, httpClient);

			try
			{
				Configure(wrapped.Configuration, context);

				var decorated = Decorate(wrapped, context) ?? wrapped;

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

		private IHalHttpClient CreateHalHttpClient(HttpClient httpClient, T context)
		{
			var wrapped = new HalHttpClient(HalJsonParser, httpClient);

			try
			{
				Configure(wrapped.Configuration, context);
				var decorated = Decorate(wrapped, context) ?? wrapped;
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
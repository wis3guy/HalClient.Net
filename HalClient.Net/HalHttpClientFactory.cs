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

		private static async Task<IRootResourceObject> GetFreshRootResourceAsync(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			if (config.BaseAddress == null)
				throw new InvalidOperationException("The root resource can only be requested for caching if the BaseAddress of the client is initialized in the Configure method of the factory.");

			return await client.GetAsync(config.BaseAddress).ConfigureAwait(false);
		}

		public IHalHttpClient CreateClient()
		{
			return CreateHalHttpClient(GetHttpClient()).Decorated;
		}

		public IHalHttpClient CreateClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			return CreateHalHttpClient(httpClient).Decorated;
		}

		public IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return CreateHalHttpClient(GetHttpClient(httpMessageHandler)).Decorated;
		}

		public async Task<IHalHttpClient> CreateClientAsync(CachingBehavior apiRootCachingBehavior)
		{
			return await CreateHalHttpClientAsync(GetHttpClient(), apiRootCachingBehavior);
		}

		public async Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, CachingBehavior apiRootCachingBehavior)
		{
			if (httpMessageHandler == null)
				throw new ArgumentNullException(nameof(httpMessageHandler));

			return await CreateHalHttpClientAsync(GetHttpClient(httpMessageHandler), apiRootCachingBehavior);
		}

		public async Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			return await CreateHalHttpClientAsync(httpClient, apiRootCachingBehavior);
		}

		private async Task<IHalHttpClient> CreateHalHttpClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior)
		{
			var created = CreateHalHttpClient(httpClient);

			switch (apiRootCachingBehavior)
			{
				case CachingBehavior.PerClient:
					var apiRootResource = await GetFreshRootResourceAsync(created.Decorated, created.Wrapped);
					created.Wrapped.CachedApiRootResource = apiRootResource;
					break;
				case CachingBehavior.Once:
					_cachedApiRootResource = _cachedApiRootResource ?? await GetFreshRootResourceAsync(created.Decorated, created.Wrapped);
					created.Wrapped.CachedApiRootResource = _cachedApiRootResource;
					break;
			}

			return created.Decorated;
		}

		private CreatedHalHttpClient CreateHalHttpClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			var halHttpClient = new HalHttpClient(Parser, httpClient);

			try
			{
				Configure(halHttpClient);

				return new CreatedHalHttpClient(Decorate(halHttpClient), halHttpClient);
			}
			catch (Exception)
			{
				halHttpClient.Dispose(); // client is unusable ...
				throw;
			}
		}
	}

	internal class CreatedHalHttpClient
	{
		public CreatedHalHttpClient(IHalHttpClient decorated, HalHttpClient wrapped)
		{
			if (decorated == null)
				throw new ArgumentNullException(nameof(decorated));

			if (wrapped == null)
				throw new ArgumentNullException(nameof(wrapped));

			Decorated = decorated;
			Wrapped = wrapped;
		}

		public IHalHttpClient Decorated { get; }
		public HalHttpClient Wrapped { get; }
	}

	public abstract class HalHttpClientFactory<T> : HalHttpClientFactory, IHalHttpClientFactory<T>
	{
		protected HalHttpClientFactory(IHalJsonParser parser) : base(parser)
		{
		}

		protected abstract void Configure(IHalHttpClientConfiguration config, T context);

		protected abstract IHalHttpClient Decorate(IHalHttpClient original, T context);

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

				var decorated = Decorate(halHttpClient, context);

				halHttpClient.CachedApiRootResource = GetApiRootResource(decorated, halHttpClient);

				return decorated;
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
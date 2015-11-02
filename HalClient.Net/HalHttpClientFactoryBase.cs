using System;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public abstract class HalHttpClientFactoryBase
	{
		protected IHalJsonParser Parser;
		private IRootResourceObject _cachedApiRootResource;

		protected HalHttpClientFactoryBase(IHalJsonParser parser)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			Parser = parser;
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

			IRootResourceObject resource = null;

			client.GetAsync(config.BaseAddress)
				.ContinueWith(x => resource = x.Result, TaskContinuationOptions.NotOnFaulted)
				.Wait();

			return resource;
		}
	}
}
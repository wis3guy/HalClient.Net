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
                throw new ArgumentNullException("parser");

            Parser = parser;
        }

        protected IRootResourceObject GetApiRootResource(IHalHttpClient client, IHalHttpClientConfiguration config)
        {
            if (config.ApiRootResourceCachingBehavior == CachingBehavior.Never)
                return null;

            switch (config.ApiRootResourceCachingBehavior)
            {
                case CachingBehavior.Once:
                    return GetCachedRootResource(client, config);
                case CachingBehavior.PerClient:
                    return GetFreshRootResource(client, config);
                default:
                    throw new NotSupportedException($"Unsupported caching behavior: {config.ApiRootResourceCachingBehavior}");
            }
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

            try
            {
                IRootResourceObject resource = null;

                // Opted to await here, rather than in the consuming code of the derived concrete factory. 
                // Motivation is that we will not *always* have to communicate with the API upon 
                // client creation; depends on IHalHttpClientConfiguration.ApiRootResourceCachingBehavior.

                var task = client
                    .GetAsync(config.BaseAddress)
                    .ContinueWith(x => resource = x.Result, TaskContinuationOptions.NotOnFaulted);

                task.ConfigureAwait(false);
                task.Wait();

                return resource;
            }
            catch (AggregateException e)
            {
                throw new Exception("Failed to retreive a fresh API root resource", e);
            }
        }
    }
}
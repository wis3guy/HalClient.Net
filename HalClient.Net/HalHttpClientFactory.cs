using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
    public class HalHttpClientFactory : IHalHttpClientFactory
    {
        private readonly IHalJsonParser _parser;
        private IRootResourceObject _cachedApiRootResource;

        public HalHttpClientFactory(IHalJsonParser parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");

            _parser = parser;
        }

        protected virtual void Configure(IHalHttpClientConfiguration config)
        {
            // Do nothing by default ...
        }

        protected virtual IHalHttpClient Transform(IHalHttpClient original)
        {
            return original; // return original by default ...
        }

        public IHalHttpClient CreateClient(HttpClient customHttpClient = null)
        {
            var httpClient = customHttpClient ?? new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});

            var client = new HalHttpClient(_parser, httpClient);

            try
            {
                Configure(client);

                if (client.ApiRootResourceCachingBehavior != CachingBehavior.Never)
                {
                    switch (client.ApiRootResourceCachingBehavior)
                    {
                        case CachingBehavior.Once:
                            client.CachedApiRootResource = GetCachedRootResource(client);
                            break;
                        case CachingBehavior.PerClient:
                            client.CachedApiRootResource = GetFreshRootResource(client);
                            break;
                    }
                }

                return Transform(client);
            }
            catch (Exception)
            {
                client.Dispose(); // client is unusable ...
                throw;
            }
        }

        private IRootResourceObject GetCachedRootResource(HalHttpClient client)
        {
            _cachedApiRootResource = _cachedApiRootResource ?? GetFreshRootResource(client);

            return _cachedApiRootResource;
        }

        private static IRootResourceObject GetFreshRootResource(HalHttpClient client)
        {
            if (client.BaseAddress == null)
                throw new InvalidOperationException("The root resource can only be requested for caching if the BaseAddress of the client is initialized in the Configure method of the factory.");

            IRootResourceObject resource = null;

            client.GetAsync(client.BaseAddress)
                .ContinueWith(x => resource = x.Result, TaskContinuationOptions.NotOnFaulted)
                .Wait();

            return resource;
        }
    }
}

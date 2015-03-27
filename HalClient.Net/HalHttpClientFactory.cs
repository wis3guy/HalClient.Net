using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
    public class HalHttpClientFactory : IHalHttpClientFactory, IHalHttpClientWithRootFactory
    {
        private readonly IHalJsonParser _parser;
        private IRootResourceObject _root;

        public HalHttpClientFactory(IHalJsonParser parser)
        {
            if (parser == null) 
                throw new ArgumentNullException("parser");
            
            _parser = parser;
        }

        protected virtual void Configure(IHalHttpClientConfiguration client)
        {
            // Do nothing by default ...
        }

        public IHalHttpClient CreateClient(HttpClient httpClient)
        {
            return CreateHalHttpClient(httpClient);
        }

        public IHalHttpClientWithRoot CreateClientWithRoot(bool refresh = false)
        {
            var client = CreateHalHttpClient();

            if (client.BaseAddress == null)
                throw new InvalidOperationException("Base address unknown. Consider creating an custom HalHttpClientFactory, and override the Configure() method. Alternatively, call this method's overload which takes the base address as a parameter.");
            
            SetRoot(client, client.BaseAddress, refresh);

            return client;
        }

        public IHalHttpClientWithRoot CreateClientWithRoot(Uri baseAddress, bool refresh = false)
        {
            if (baseAddress == null)
                throw new ArgumentNullException("baseAddress");
            
            var client = CreateHalHttpClient();

            SetRoot(client, baseAddress, refresh);

            return client;
        }

        private void SetRoot(HalHttpClient client, Uri baseAddress, bool refresh)
        {
            try
            {
                if (refresh || (_root == null))
                    client.GetAsync(baseAddress)
                        .ContinueWith(x => _root = x.Result, TaskContinuationOptions.NotOnFaulted)
                        .Wait();

                client.Root = _root;
            }
            catch (AggregateException ex)
            {
                client.Dispose(); // client is unusable ...
                throw new Exception("Could not GET the root response from '" + baseAddress + "'.", ex.InnerExceptions.FirstOrDefault());
            }
        }

        private HalHttpClient CreateHalHttpClient(HttpClient httpClient = null)
        {
            var client = new HalHttpClient(_parser, httpClient);

            try
            {
                Configure(client);
                return client;
            }
            catch (Exception)
            {
                client.Dispose(); // client is unusable ...
                throw;
            }
        }
    }
}

using System;
using HalClient.Net.Parser;

namespace HalClient.Net
{
    public class HalHttpClientFactory : IHalHttpClientFactory
    {
        private readonly IHalJsonParser _parser;

        public HalHttpClientFactory(IHalJsonParser parser)
        {
            if (parser == null) 
                throw new ArgumentNullException("parser");
            
            _parser = parser;
        }

        protected virtual void Configure(IHalHttpClientConfiguration client)
        {
        }

        public IHalHttpClient CreateClient()
        {
            var client = new HalHttpClient(_parser);

            try
            {
                Configure(client);
                return client;
            }
            catch (Exception)
            {
                client.Dispose(); // an exception was thrown during configuration, client is unusable ...
                throw;
            }
        }
    }
}

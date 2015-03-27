using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
    internal class HalHttpClient : IHalHttpClientConfiguration, IHalHttpClientWithRoot
    {
        private readonly IHalJsonParser _parser;
        private HttpClient _client;

        internal HalHttpClient(IHalJsonParser parser, HttpClient client)
        {
            if (parser == null) throw new ArgumentNullException("parser");
            _parser = parser;
            _client = client ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        }

        public Uri BaseAddress
        {
            get { return _client.BaseAddress; }
            set { _client.BaseAddress = value; }
        }

        public long MaxResponseContentBufferSize
        {
            get { return _client.MaxResponseContentBufferSize; }
            set { _client.MaxResponseContentBufferSize = value; }
        }

        public TimeSpan Timeout
        {
            get { return _client.Timeout; }
            set { _client.Timeout = value; }
        }

        public HttpRequestHeaders Headers
        {
            get { return _client.DefaultRequestHeaders; }
        }

        public async Task<IRootResourceObject> PostAsync<T>(Uri uri, T data)
        {
            ResetAcceptHeader();
            
            var response = await _client.PostAsJsonAsync(uri, data);
            
            return await ProcessResponseMessage(response);
        }

        public async Task<IRootResourceObject> PutAsync<T>(Uri uri, T data)
        {
            ResetAcceptHeader();

            var response = await _client.PutAsJsonAsync(uri, data);

            return await ProcessResponseMessage(response);
        }

        public async Task<IRootResourceObject> GetAsync(Uri uri)
        {
            ResetAcceptHeader();

            var response = await _client.GetAsync(uri);

            return await ProcessResponseMessage(response);
        }

        public async Task<IRootResourceObject> DeleteAsync(Uri uri)
        {
            ResetAcceptHeader();

            var response = await _client.DeleteAsync(uri);

            return await ProcessResponseMessage(response);
        }

        private async Task<IRootResourceObject> ProcessResponseMessage(HttpResponseMessage response)
        {
            if ((response.StatusCode == HttpStatusCode.Redirect) ||
                (response.StatusCode == HttpStatusCode.SeeOther) ||
                (response.StatusCode == HttpStatusCode.RedirectMethod))
                return await GetAsync(response.Headers.Location);

            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.NoContent)
                return new RootResourceObject();

            var json = await response.Content.ReadAsStringAsync();

            return _parser.ParseResource(json);
        }

        private void ResetAcceptHeader()
        {
            // FUTURE: Add support for application/hal+xml

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("Accept", "application/hal+json");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) 
                return;

            if (_client == null) 
                return;

            _client.Dispose();
            _client = null;
        }

        public IRootResourceObject Root { get; set; }
    }
}
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HalClient.Net
{
	internal class NonParsingHttpClient : INonParsingHttpClient
	{
		private readonly HttpClient _httpClient;

		public NonParsingHttpClient(HttpClient httpClient)
		{
			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			this._httpClient = httpClient;
		}

		public Task<HttpResponseMessage> PostAsJsonAsync<T>(Uri uri, T data)
		{
			return _httpClient.PostAsJsonAsync(uri, data);
		}

		public Task<HttpResponseMessage> PutAsJsonAsync<T>(Uri uri, T data)
		{
			return _httpClient.PutAsJsonAsync(uri, data);
		}

		public Task<HttpResponseMessage> GetAsync(Uri uri)
		{
			return _httpClient.GetAsync(uri);
		}

		public Task<HttpResponseMessage> DeleteAsync(Uri uri)
		{
			return _httpClient.DeleteAsync(uri);
		}

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			return _httpClient.SendAsync(request);
		}
	}
}
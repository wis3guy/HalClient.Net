using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace HalClient.Net
{
	public interface INonParsingHttpClient
	{
		Task<HttpResponseMessage> PostAsJsonAsync<T>(Uri uri, T data);
		Task<HttpResponseMessage> PutAsJsonAsync<T>(Uri uri, T data);
		Task<HttpResponseMessage> GetAsync(Uri uri);
		Task<HttpResponseMessage> DeleteAsync(Uri uri);
		Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
	}
}
using System.Net.Http;

namespace HalClient.Net
{
	public interface IHalHttpClientFactory
	{
		IHalHttpClient CreateClient();
		IHalHttpClient CreateClient(HttpClient httpClient);
		IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler);
	}

	public interface IHalHttpClientFactory<in T> : IHalHttpClientFactory
	{
		IHalHttpClient CreateClient(T context);
		IHalHttpClient CreateClient(HttpClient httpClient, T context);
		IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler, T context);
	}
}
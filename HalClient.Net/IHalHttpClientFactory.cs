using System.Net.Http;

namespace HalClient.Net
{
	public interface IHalHttpClientFactory
	{
		IHalHttpClient CreateClient();
		IHalHttpClient CreateClient(HttpClient customHttpClient);
	}

	public interface IHalHttpClientFactory<in T>
	{
		IHalHttpClient CreateClient(T context);
		IHalHttpClient CreateClient(HttpClient customHttpClient, T context);
	}
}
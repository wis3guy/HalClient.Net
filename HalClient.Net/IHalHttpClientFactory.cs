using System.Net.Http;
using System.Threading.Tasks;

namespace HalClient.Net
{
	public interface IHalHttpClientFactory
	{
		IHalHttpClient CreateClient();
		IHalHttpClient CreateClient(HttpClient httpClient);
		IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler);

		Task<IHalHttpClient> CreateClientAsync(CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
		Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
		Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
	}

	public interface IHalHttpClientFactory<in T> : IHalHttpClientFactory
	{
		IHalHttpClient CreateClient(T context);
		IHalHttpClient CreateClient(HttpClient httpClient, T context);
		IHalHttpClient CreateClient(HttpMessageHandler httpMessageHandler, T context);

		Task<IHalHttpClient> CreateClientAsync(T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
		Task<IHalHttpClient> CreateClientAsync(HttpClient httpClient, T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
		Task<IHalHttpClient> CreateClientAsync(HttpMessageHandler httpMessageHandler, T context, CachingBehavior apiRootCachingBehavior = CachingBehavior.Never);
	}
}
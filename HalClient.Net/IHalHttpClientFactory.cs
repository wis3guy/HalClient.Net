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
}
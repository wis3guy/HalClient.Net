using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public interface IHalHttpClient : IDisposable
	{
		Task<IHalHttpResponseMessage> PostAsync<T>(Uri uri, T data);
		Task<IHalHttpResponseMessage> PutAsync<T>(Uri uri, T data);
		Task<IHalHttpResponseMessage> GetAsync(Uri uri);
		Task<IHalHttpResponseMessage> DeleteAsync(Uri uri);
		Task<IHalHttpResponseMessage> SendAsync(HttpRequestMessage request);
		IRootResourceObject CachedApiRootResource { get; }
		HttpClient HttpClient { get; }
	}
}
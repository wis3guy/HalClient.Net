using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public interface IHalHttpClient : IDisposable
	{
		Task<IRootResourceObject> PostAsync<T>(Uri uri, T data);
		Task<IRootResourceObject> PutAsync<T>(Uri uri, T data);
		Task<IRootResourceObject> GetAsync(Uri uri);
		Task<IRootResourceObject> DeleteAsync(Uri uri);
		Task<IRootResourceObject> SendAsync(HttpRequestMessage request);
		IRootResourceObject CachedApiRootResource { get; }
		HttpClient HttpClient { get; }
	}
}
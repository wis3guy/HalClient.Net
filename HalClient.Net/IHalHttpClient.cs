using System;
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
        IRootResourceObject CachedApiRootResource { get; }
    }
}
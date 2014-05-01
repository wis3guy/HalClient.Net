using HalClient.Net.Parser;

namespace HalClient.Net
{
    public interface IHalHttpClientWithRoot : IHalHttpClient
    {
        IRootResourceObject Root { get; }
    }
}
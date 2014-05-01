using System;

namespace HalClient.Net
{
    public interface IHalHttpClientWithRootFactory
    {
        IHalHttpClientWithRoot CreateClientWithRoot(bool refresh = false);
        IHalHttpClientWithRoot CreateClientWithRoot(Uri baseAddress, bool refresh = false);
    }
}
using System;
using System.Net.Http.Headers;

namespace HalClient.Net
{
    public interface IHalHttpClientConfiguration : IDisposable
    {
        Uri BaseAddress { get; set; }
        long MaxResponseContentBufferSize { get; set; }
        TimeSpan Timeout { get; set; }
        HttpRequestHeaders Headers { get; }
        ResponseParseBehavior ParseBehavior { get; set; }
    }
}
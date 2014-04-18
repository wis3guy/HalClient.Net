namespace HalClient.Net
{
    public interface IHalHttpClientFactory
    {
        IHalHttpClient CreateClient();
    }
}
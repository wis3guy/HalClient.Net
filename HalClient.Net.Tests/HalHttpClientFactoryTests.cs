using HalClient.Net.Parser;
using Xunit;

namespace HalClient.Net.Tests
{
    public class HalHttpClientFactoryTests
    {
        readonly IHalHttpClientFactory _target = new HalHttpClientFactory(new HalJsonParser());

        [Fact]
        public void CreateClient_CreatesAClient()
        {
            using (var client = _target.CreateClient())
            {
                Assert.NotNull(client);
            }
        }
    }
}

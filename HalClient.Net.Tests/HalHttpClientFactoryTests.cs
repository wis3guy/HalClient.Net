using System.Net.Http;
using HalClient.Net.Parser;
using Moq;
using Xunit;

namespace HalClient.Net.Tests
{
	public class HalHttpClientFactoryTests
	{
		private readonly IHalJsonParser _mockedHalJsonParser = new Mock<IHalJsonParser>().Object;

		[Fact]
		public void CreateClient_CreatesAClient()
		{
			var sut = new HalHttpClientFactory(_mockedHalJsonParser);

			using (var client = sut.CreateClient())
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_WithHttpClient()
		{
			var sut = new HalHttpClientFactory(_mockedHalJsonParser);

			using (var httpClient = new HttpClient())
			using (var client = sut.CreateClient(httpClient))
			{
				Assert.NotNull(client);
			}
		}
	}
}
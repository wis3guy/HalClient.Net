using System;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public abstract class HalHttpClientFactoryBase
	{
		protected readonly IHalJsonParser HalJsonParser;

		protected HalHttpClientFactoryBase(IHalJsonParser parser)
		{
			HalJsonParser = parser ?? throw new ArgumentNullException(nameof(parser));
		}

		protected virtual HttpClient GetHttpClient()
		{
			return new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});
		}

		protected virtual HttpClient GetHttpClient(HttpMessageHandler httpMessageHandler)
		{
			return new HttpClient(httpMessageHandler);
		}

		protected static async Task<IRootResourceObject> GetFreshRootResourceAsync(IHalHttpClient client, IHalHttpClientConfiguration config)
		{
			if (config.BaseAddress == null)
				throw new InvalidOperationException("The root resource can only be requested for caching if the BaseAddress of the client is initialized in the Configure method of the factory.");

			var message = await client.GetAsync(config.BaseAddress).ConfigureAwait(false);

			return message.Resource;
		}
	}
}
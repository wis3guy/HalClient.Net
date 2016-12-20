using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	internal class HalHttpResponseMessage : IHalHttpResponseMessage
	{
		private HalHttpResponseMessage(HttpResponseMessage response)
		{
			StatusCode = response.StatusCode;
			Location = response.Headers.Location;
			IsSuccessStatusCode = response.IsSuccessStatusCode;
			ReasonPhrase = response.ReasonPhrase;

			if (response.Content.Headers.ContentType == null)
				return;

			Mediatype = response.Content.Headers.ContentType.MediaType;
			IsHalResponse = Mediatype.Equals(MediaType.ApplicationHalPlusJson, StringComparison.OrdinalIgnoreCase);
		}

		public string ReasonPhrase { get; }
		public bool IsSuccessStatusCode { get; }
		public Uri Location { get; }
		public HttpStatusCode StatusCode { get; }
		public string Mediatype { get; }
		public bool IsHalResponse { get; }
		public string Content { get; private set; }
		public IRootResourceObject Resource { get; private set; }

		public static async Task<IHalHttpResponseMessage> CreateAsync(HttpResponseMessage response, IHalJsonParser parser)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			var message = new HalHttpResponseMessage(response)
			{
				Content = await response.Content.ReadAsStringAsync()
			};

			if (message.IsHalResponse)
			{
				var result = parser.Parse(message.Content);
				message.Resource = new RootResourceObject(result);
			}

			return message;
		}
	}
}
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	internal class HalHttpClient : IHalHttpClientConfiguration, IHalHttpClient
	{
		private const string ApplicationHalJson = "application/hal+json";
		private readonly IHalJsonParser _parser;
		private HttpClient _client;

		internal HalHttpClient(IHalJsonParser parser, HttpClient client)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			if (client == null)
				throw new ArgumentNullException(nameof(client));

			_parser = parser;
			_client = client;
		}

		public Uri BaseAddress
		{
			get { return _client.BaseAddress; }
			set { _client.BaseAddress = value; }
		}

		public long MaxResponseContentBufferSize
		{
			get { return _client.MaxResponseContentBufferSize; }
			set { _client.MaxResponseContentBufferSize = value; }
		}

		public TimeSpan Timeout
		{
			get { return _client.Timeout; }
			set { _client.Timeout = value; }
		}

		public HttpRequestHeaders Headers => _client.DefaultRequestHeaders;

		public CachingBehavior ApiRootResourceCachingBehavior { get; set; }

		public async Task<IRootResourceObject> PostAsync<T>(Uri uri, T data)
		{
			ResetAcceptHeader();

			var response = await _client.PostAsJsonAsync(uri, data);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> PutAsync<T>(Uri uri, T data)
		{
			ResetAcceptHeader();

			var response = await _client.PutAsJsonAsync(uri, data);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> GetAsync(Uri uri)
		{
			ResetAcceptHeader();

			var response = await _client.GetAsync(uri);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> DeleteAsync(Uri uri)
		{
			ResetAcceptHeader();

			var response = await _client.DeleteAsync(uri);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> SendAsync(HttpRequestMessage request)
		{
			ResetAcceptHeader();

			var response = await _client.SendAsync(request);

			return await ProcessResponseMessage(response);
		}

		public IRootResourceObject CachedApiRootResource { get; set; }

		private async Task<IRootResourceObject> ProcessResponseMessage(HttpResponseMessage response)
		{
			if ((response.StatusCode == HttpStatusCode.Redirect) ||
				(response.StatusCode == HttpStatusCode.SeeOther) ||
				(response.StatusCode == HttpStatusCode.RedirectMethod))
				return await GetAsync(response.Headers.Location);

			string mediatype = null;
			var isHalResponse = false;

			if (response.Content.Headers.ContentType != null)
			{
				mediatype = response.Content.Headers.ContentType.MediaType;
				isHalResponse = mediatype.Equals(ApplicationHalJson, StringComparison.OrdinalIgnoreCase);
			}

			try
			{
				response.EnsureSuccessStatusCode();

				if (response.StatusCode == HttpStatusCode.NoContent)
					return new RootResourceObject();

				if (string.IsNullOrEmpty(mediatype))
					throw new NotSupportedException("The response is missing the 'Content-Type' header");

				if (!isHalResponse)
					throw new NotSupportedException("The response contains an unsupported 'Content-Type' header value: " + mediatype);

				var resource = await ParseContent(response);

				return resource;
			}
			catch (HttpRequestException e)
			{
				if (!isHalResponse)
					throw;

				var resource = await ParseContent(response);

				throw new HalHttpRequestException(e.Message, e, resource);
			}
		}

		private async Task<RootResourceObject> ParseContent(HttpResponseMessage response)
		{
			var json = await response.Content.ReadAsStringAsync();
			var result = _parser.Parse(json);

			return new RootResourceObject(result);
		}

		private void ResetAcceptHeader()
		{
			// FUTURE: Add support for application/hal+xml

			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Add("Accept", ApplicationHalJson);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			if (_client == null)
				return;

			_client.Dispose();
			_client = null;
		}
	}

	[Serializable]
	public class HalHttpRequestException : Exception
	{
		public HalHttpRequestException(string message, IRootResourceObject resource = null)
			: base(message)
		{
			Resource = resource;
		}

		public HalHttpRequestException(string message, Exception inner, IRootResourceObject resource = null)
			: base(message, inner)
		{
			Resource = resource;
		}

		public IRootResourceObject Resource { get; }
	}
}
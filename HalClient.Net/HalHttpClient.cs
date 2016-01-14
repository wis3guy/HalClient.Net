using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	internal class HalHttpClient : IHalHttpClient
	{
		private const string ApplicationHalJson = "application/hal+json";
		private readonly IHalJsonParser _parser;
		private HttpClient _httpClient;

		internal HalHttpClient(IHalJsonParser parser, HttpClient httpClient)
		{
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));

			if (httpClient == null)
				throw new ArgumentNullException(nameof(httpClient));

			_parser = parser;
			_httpClient = httpClient;

			Configuration = new HalHttpClientConfiguration(httpClient);
		}

		public IHalHttpClientConfiguration Configuration { get; }
		
		public async Task<IRootResourceObject> PostAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.PostAsJsonAsync(uri, data);
			
			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> PutAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.PutAsJsonAsync(uri, data);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> GetAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.GetAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> DeleteAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.DeleteAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IRootResourceObject> SendAsync(HttpRequestMessage request)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.SendAsync(request);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public IRootResourceObject CachedApiRootResource { get; set; }

		public HttpClient HttpClient => _httpClient;

		private void RestoreAcceptHeaders(IEnumerable<MediaTypeWithQualityHeaderValue> backup)
		{
			Configuration.Headers.Accept.Clear();

			foreach (var headerValue in backup)
				Configuration.Headers.Accept.Add(headerValue);
		}

		private MediaTypeWithQualityHeaderValue[] OverrideAcceptHeaders()
		{
			var backup = Configuration.Headers.Accept.ToArray();

			Configuration.Headers.Accept.Clear();
			Configuration.Headers.Add("Accept", ApplicationHalJson);

			return backup;
		}

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

			if (response.IsSuccessStatusCode)
			{
				if (response.StatusCode == HttpStatusCode.NoContent)
					return new RootResourceObject();

				if (string.IsNullOrEmpty(mediatype))
					throw new NotSupportedException("The response is missing the 'Content-Type' header");

				if (!isHalResponse)
					throw new NotSupportedException("The response contains an unsupported 'Content-Type' header value: " + mediatype);

				return await ParseContentAsync(response);
			}

			if (!isHalResponse)
				throw new HalHttpRequestException(response.StatusCode, response.ReasonPhrase);

			var resource = await ParseContentAsync(response);

			throw new HalHttpRequestException(response.StatusCode, response.ReasonPhrase, resource);
		}

		private async Task<IRootResourceObject> ParseContentAsync(HttpResponseMessage response)
		{
			var json = await response.Content.ReadAsStringAsync();
			var result = _parser.Parse(json);

			return new RootResourceObject(result);
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

			if (_httpClient == null)
				return;

			_httpClient.Dispose();
			_httpClient = null;
		}
	}
}
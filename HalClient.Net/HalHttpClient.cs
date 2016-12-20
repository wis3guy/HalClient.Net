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
		
		public async Task<IHalHttpResponseMessage> PostAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.PostAsJsonAsync(uri, data);
			
			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> PutAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.PutAsJsonAsync(uri, data);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> GetAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.GetAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> DeleteAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await _httpClient.DeleteAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> SendAsync(HttpRequestMessage request)
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
			Configuration.Headers.Add("Accept", MediaType.ApplicationHalPlusJson);

			return backup;
		}

		private async Task<IHalHttpResponseMessage> ProcessResponseMessage(HttpResponseMessage response)
		{
			if (Configuration.AutoFollowRedirects &&
			    ((response.StatusCode == HttpStatusCode.Redirect) ||
			     (response.StatusCode == HttpStatusCode.SeeOther) ||
			     (response.StatusCode == HttpStatusCode.RedirectMethod)))
				return await GetAsync(response.Headers.Location);

			var message = await HalHttpResponseMessage.CreateAsync(response, _parser);

			if (message.IsSuccessStatusCode)
				return message;

			throw new HalHttpRequestException(message);
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
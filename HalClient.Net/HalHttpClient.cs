using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HalClient.Net.Parser;
using Newtonsoft.Json;

namespace HalClient.Net
{
	internal class HalHttpClient : IHalHttpClient
	{
		private readonly IHalJsonParser _parser;

		internal HalHttpClient(IHalJsonParser parser, HttpClient httpClient)
		{
			_parser = parser ?? throw new ArgumentNullException(nameof(parser));
			HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

			Configuration = new HalHttpClientConfiguration(httpClient);
		}

		public IHalHttpClientConfiguration Configuration { get; }

		public async Task<IHalHttpResponseMessage> PostAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await HttpClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> PutAsync<T>(Uri uri, T data)
		{
			var backup = OverrideAcceptHeaders();
			var response = await HttpClient.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> GetAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await HttpClient.GetAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> DeleteAsync(Uri uri)
		{
			var backup = OverrideAcceptHeaders();
			var response = await HttpClient.DeleteAsync(uri);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public async Task<IHalHttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			var backup = OverrideAcceptHeaders();
			var response = await HttpClient.SendAsync(request);

			RestoreAcceptHeaders(backup);

			return await ProcessResponseMessage(response);
		}

		public IRootResourceObject CachedApiRootResource { get; set; }

		public HttpClient HttpClient { get; private set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void RestoreAcceptHeaders(IEnumerable<MediaTypeWithQualityHeaderValue> backup)
		{
			Configuration.Headers.Accept.Clear();

			foreach (var headerValue in backup)
				Configuration.Headers.Accept.Add(headerValue);
		}

		private IEnumerable<MediaTypeWithQualityHeaderValue> OverrideAcceptHeaders()
		{
			var backup = Configuration.Headers.Accept.ToArray();

			Configuration.Headers.Accept.Clear();
			Configuration.Headers.Add("Accept", MediaType.ApplicationHalPlusJson);

			return backup;
		}

		private async Task<IHalHttpResponseMessage> ProcessResponseMessage(HttpResponseMessage response)
		{
			if (Configuration.AutoFollowRedirects &&
			    (response.StatusCode == HttpStatusCode.Redirect ||
			     response.StatusCode == HttpStatusCode.SeeOther ||
			     response.StatusCode == HttpStatusCode.RedirectMethod))
				return await GetAsync(response.Headers.Location);

			var message = await HalHttpResponseMessage.CreateAsync(response, _parser);

			if (response.IsSuccessStatusCode || !Configuration.ThrowOnError)
				return message;

			throw new HalHttpRequestException(response.StatusCode, response.ReasonPhrase, message.Resource);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			if (HttpClient == null)
				return;

			HttpClient.Dispose();
			HttpClient = null;
		}

		~HalHttpClient()
		{
			Dispose(false);
		}
	}
}
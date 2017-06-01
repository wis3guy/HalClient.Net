using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HalClient.Net
{
	public class HalHttpClientConfiguration : IHalHttpClientConfiguration
	{
		private readonly HttpClient _httpClient;

		public HalHttpClientConfiguration(HttpClient httpClient)
		{
			_httpClient = httpClient;
			AutoFollowRedirects = true;
			ThrowOnError = true;
		}

		public Uri BaseAddress
		{
			get => _httpClient.BaseAddress;
			set => _httpClient.BaseAddress = value;
		}

		public long MaxResponseContentBufferSize
		{
			get => _httpClient.MaxResponseContentBufferSize;
			set => _httpClient.MaxResponseContentBufferSize = value;
		}

		public TimeSpan Timeout
		{
			get => _httpClient.Timeout;
			set => _httpClient.Timeout = value;
		}

		public bool AutoFollowRedirects { get; set; }
		public bool ThrowOnError { get; set; }

		public HttpRequestHeaders Headers => _httpClient.DefaultRequestHeaders;
	}
}
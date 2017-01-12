using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HalClient.Net
{
	public interface IHalHttpClientConfiguration
	{
		/// <summary>
		/// <see cref="HttpClient.BaseAddress"/>
		/// </summary>
		Uri BaseAddress { get; set; }

		/// <summary>
		/// <see cref="HttpClient.MaxResponseContentBufferSize"/>
		/// </summary>
		long MaxResponseContentBufferSize { get; set; }

		/// <summary>
		/// <see cref="HttpClient.Timeout"/>
		/// </summary>
		TimeSpan Timeout { get; set; }

		/// <summary>
		/// <see cref="HttpClient.DefaultRequestHeaders"/>
		/// </summary>
		HttpRequestHeaders Headers { get; }

		/// <summary>
		/// Wether the client should automatically follow (ie. perform a subsequent GET request) in case the server returns either an HTTP 302 or 303 status code.
		/// Default value is true.
		/// </summary>
		bool AutoFollowRedirects { get; set; }

		/// <summary>
		/// Wether a <see cref="HalHttpRequestException"/> should be thrown upon receiving a non-success response from the server.
		/// Default value is true.
		/// </summary>
		bool ThrowOnError { get; set; }
	}
}
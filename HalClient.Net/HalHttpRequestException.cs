using System;
using System.Net;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	[Serializable]
	public class HalHttpRequestException : Exception
	{
		public HalHttpRequestException(HttpStatusCode statusCode, string reason, IRootResourceObject resource = null)
			: base($"{(int)statusCode} ({reason})")
		{
			StatusCode = statusCode;
			Resource = resource;
		}
		public HttpStatusCode StatusCode { get; }
		public IRootResourceObject Resource { get; }
	}
}
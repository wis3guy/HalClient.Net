using System;
using System.Net;
using HalClient.Net.Parser;

namespace HalClient.Net
{
	public interface IHalHttpResponseMessage
	{
		string ReasonPhrase { get; }
		bool IsSuccessStatusCode { get; }
		Uri Location { get; }
		HttpStatusCode StatusCode { get; }
		string Mediatype { get; }
		bool IsHalResponse { get; }
		string Content { get; }
		IRootResourceObject Resource { get; }
	}
}
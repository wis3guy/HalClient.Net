using System;
using System.Net;

namespace HalClient.Net.Parser
{
	public interface IRootResourceObject : IResourceObject
	{
		Uri GetDocumentationUri(IHaveLinkRelation link);

		HttpStatusCode StatusCode { get; }
	}
}
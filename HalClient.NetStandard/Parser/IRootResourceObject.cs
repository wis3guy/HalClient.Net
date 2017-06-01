using System;

namespace HalClient.Net.Parser
{
	public interface IRootResourceObject : IResourceObject
	{
		Uri GetDocumentationUri(IHaveLinkRelation link);
	}
}
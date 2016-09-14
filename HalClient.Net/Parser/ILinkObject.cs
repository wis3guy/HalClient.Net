using System;
using Tavis.UriTemplates;

namespace HalClient.Net.Parser
{
	public interface ILinkObject : IHaveLinkRelation, IHaveCustomAttributes
	{
		Uri Href { get; }
		bool Templated { get; }
		string Template { get; }
		string Type { get; }
		Uri Deprecation { get; }
		string Name { get; }
		Uri Profile { get; }
		string Title { get; }
		string HrefLang { get; }
		ILinkObject ResolveTemplated(Func<UriTemplate, string> hrefResolver);
		// FUTURE: Support multiple of these as specified here: http://tools.ietf.org/html/rfc5988#section-5.4 
		// FUTURE: Curies
		// FUTURE: Rel as Uri
		// FUTURE: IANA rels
	}
}
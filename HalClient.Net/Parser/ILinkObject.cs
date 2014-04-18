using System;

namespace HalClient.Net.Parser
{
    public interface ILinkObject : IHaveLinkRelation
    {
        Uri Href { get; }
        bool Templated { get; }
        string Type { get; }
        Uri Deprecation { get; }
        string Name { get; }
        Uri Profile { get; }
        string Title { get; }
        string HrefLang { get; } // FUTURE: Support multiple of these as specified here: http://tools.ietf.org/html/rfc5988#section-5.4 

        // TODO: Curies
        // TODO: Rel as Uri
        // TODO: IANA rels
    }
}
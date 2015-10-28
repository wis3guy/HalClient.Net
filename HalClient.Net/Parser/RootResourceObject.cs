using System;
using System.Net;
using Tavis.UriTemplates;

namespace HalClient.Net.Parser
{
    internal class RootResourceObject : ResourceObjectBase, IRootResourceObject
    {
        public RootResourceObject(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public RootResourceObject(HttpStatusCode statusCode, HalJsonParseResult result)
            : base(result.StateValues, result.EmbeddedResources, result.Links)
        {
            StatusCode = statusCode;
        }

        public Uri GetDocumentationUri(IHaveLinkRelation subject)
        {
            if (!Links.ContainsKey("curies"))
                return null;

            foreach (var link in Links["curies"])
            {
                var prefix = string.Format("{0}:", link.Name);

                if (!subject.Rel.StartsWith(prefix)) 
                    continue;

                var parts = subject.Rel.Split(':');

                if (parts.Length > 2)
                    throw new FormatException("Invalid, named link relation:" + subject.Rel);

                return ResolveDocumentationUri(link, parts[1]);
            }

            return null;
        }

        public HttpStatusCode StatusCode { get; private set; }

        private static Uri ResolveDocumentationUri(ILinkObject link, string rel)
        {
            var template = new UriTemplate(link.Href.ToString());

            template.SetParameter("rel", rel);

            return new Uri(template.Resolve());
        }
    }
}
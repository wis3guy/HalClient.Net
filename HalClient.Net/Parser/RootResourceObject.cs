using System;
using Tavis.UriTemplates;

namespace HalClient.Net.Parser
{
	internal class RootResourceObject : ResourceObjectBase, IRootResourceObject
	{
		public RootResourceObject()
		{
		}

		public RootResourceObject(HalJsonParseResult result)
			: base(result.StateValues, result.EmbeddedResources, result.Links)
		{
		}

		public Uri GetDocumentationUri(IHaveLinkRelation subject)
		{
			if (!Links.ContainsKey("curies"))
				return null;

			foreach (var link in Links["curies"])
			{
				var prefix = $"{link.Name}:";

				if (!subject.Rel.StartsWith(prefix))
					continue;

				var parts = subject.Rel.Split(':');

				if (parts.Length > 2)
					throw new FormatException("Invalid, named link relation:" + subject.Rel);

				return ResolveDocumentationUri(link, parts[1]);
			}

			return null;
		}

		private static Uri ResolveDocumentationUri(ILinkObject link, string rel)
		{
			var template = new UriTemplate(link.Href.ToString());

			template.SetParameter("rel", rel);

			return new Uri(template.Resolve());
		}
	}
}
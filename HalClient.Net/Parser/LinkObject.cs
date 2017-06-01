using System;
using Tavis.UriTemplates;

namespace HalClient.Net.Parser
{
	internal class LinkObject : ILinkObject
	{
		public string Rel { get; set; }
		public Uri Href { get; private set; }
		public bool Templated { get; set; }
		public string Template { get; set; }
		public string Type { get; set; }
		public Uri Deprecation { get; private set; }
		public string Name { get; set; }
		public Uri Profile { get; private set; }
		public string Title { get; set; }
		public string HrefLang { get; set; }

		public ILinkObject ResolveTemplated(Func<UriTemplate, string> hrefResolver)
		{
			if (!Templated)
				throw new InvalidOperationException("Cannot resolve a non-Templated link");

			var template = new UriTemplate(Template);
			var href = hrefResolver(template);

			var link = new LinkObject
			{
				Rel = Rel,
				Templated = false,
				Deprecation = Deprecation,
				HrefLang = HrefLang,
				Name = Name,
				Profile = Profile,
				Title = Title,
				Type = Type
			};

			link.SetHref(href);

			return link;
		}

		public void SetHref(string href)
		{
			Href = TryCreateUri(href, UriKind.RelativeOrAbsolute);
		}

		public void SetDeprecation(string deprecationUri)
		{
			Deprecation = TryCreateUri(deprecationUri, UriKind.Absolute);
		}

		public void SetProfile(string profileUri)
		{
			Profile = TryCreateUri(profileUri, UriKind.Absolute);
		}

		private static Uri TryCreateUri(string value, UriKind kind)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			try
			{
				return new Uri(value, kind);
			}
#if !PORTABLE
			catch (UriFormatException)
#else
			catch (FormatException)
#endif
			{
				return null;
			}
		}
	}
}
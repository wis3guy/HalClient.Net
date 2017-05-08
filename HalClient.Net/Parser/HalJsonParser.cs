using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HalClient.Net.Parser
{
	public class HalJsonParser : IHalJsonParser
	{
		public HalJsonParseResult Parse(string json)
		{
			if (string.IsNullOrEmpty(json))
				throw new ArgumentNullException(nameof(json));

			var obj = JObject.Parse(json);
			var resource = ParseRootResourceObject(obj);

			return resource;
		}

		private static HalJsonParseResult ParseRootResourceObject(JObject outer)
		{
			var links = new List<ILinkObject>();
			var embedded = new List<IEmbeddedResourceObject>();
			var state = new List<IStateValue>();

			ParseResourceObject(outer, links, embedded, state);

			return new HalJsonParseResult(links, embedded, state);
		}

		private static EmbeddedResourceObject ParseEmbeddedResourceObject(JObject outer, string rel)
		{
			var links = new List<ILinkObject>();
			var embedded = new List<IEmbeddedResourceObject>();
			var state = new List<IStateValue>();

			ParseResourceObject(outer, links, embedded, state);

			return new EmbeddedResourceObject(links, embedded, state, rel);
		}

		private static void ParseResourceObject(JObject outer, List<ILinkObject> links, List<IEmbeddedResourceObject> embedded, List<IStateValue> state)
		{
			foreach (var inner in outer.Properties())
			{
				var type = inner.Value.Type.ToString();

				if (inner.Value.Type == JTokenType.Object)
				{
					var value = (JObject) inner.Value;

					switch (inner.Name)
					{
						case "_links":
							links.AddRange(ParseObjectOrArrayOfObjects(value, ParseLinkObject));
							break;
						case "_embedded":
							embedded.AddRange(ParseObjectOrArrayOfObjects(value, ParseEmbeddedResourceObject));
							break;
						default:
							state.Add(new StateValue(inner.Name, value.ToString(Formatting.Indented), type));
							break;
					}
				}
				else
				{
					var value = inner.Value.ToString();

					switch (inner.Name)
					{
						case "_links":
						case "_embedded":
							if (inner.Value.Type != JTokenType.Null)
							{
								throw new FormatException(string.Format("Invalid value for {0}: {1}", inner.Name, value));
							}
							break;
						default:
							state.Add(new StateValue(inner.Name, value, type));
							break;
					}
				}
			}
		}

		private static LinkObject ParseLinkObject(JObject outer, string rel)
		{
			var link = new LinkObject {Rel = rel};
			string href = null;

			foreach (var inner in outer.Properties())
			{
				var value = inner.Value.ToString();

				if (string.IsNullOrEmpty(value))
					continue; // nothing to assign, just leave the default value ...

				var attribute = inner.Name.ToLowerInvariant();

				switch (attribute)
				{
					case "href":
						href = value;
						break;
					case "templated":
						link.Templated = value.Equals("true", StringComparison.OrdinalIgnoreCase);
						break;
					case "type":
						link.Type = value;
						break;
					case "deprication":
						link.SetDeprecation(value);
						break;
					case "name":
						link.Name = value;
						break;
					case "profile":
						link.SetProfile(value);
						break;
					case "title":
						link.Title = value;
						break;
					case "hreflang":
						link.HrefLang = value;
						break;
                    default:
                        if(link.CustomAttributes.ContainsKey(attribute))
                            throw new NotSupportedException($"Custom link attribute '{attribute}' is defined more than once");
                        link.CustomAttributes.Add(attribute, ParseCustomValue(inner.Value));
                        break;
                }
			}

			if (link.Templated)
				link.Template = href;
			else
				link.SetHref(href);

			return link;
		}

        private static object ParseCustomValue(JToken customAttribute)
        {
            if (customAttribute.Type == JTokenType.Array)
                return customAttribute.Children().Select(x => x.Value<string>()).ToArray();
            else
                return customAttribute.Value<string>();
        }

        private static IEnumerable<T> ParseObjectOrArrayOfObjects<T>(JObject outer, Func<JObject, string, T> factory)
		{
			foreach (var inner in outer.Properties())
			{
				var rel = inner.Name;

				if (inner.Value.Type == JTokenType.Array)
					foreach (var child in inner.Value.Children<JObject>())
						yield return factory(child, rel);
				else
					yield return factory((JObject) inner.Value, rel);
			}
		}
	}
}
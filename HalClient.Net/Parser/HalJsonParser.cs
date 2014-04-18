using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HalClient.Net.Parser
{
    public class HalJsonParser : IHalJsonParser
    {
        public IRootResourceObject ParseResource(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            var obj = JObject.Parse(json);
            var resource = ParseRootResourceObject(obj);

            return resource;
        }

        private static RootResourceObject ParseRootResourceObject(JObject outer)
        {
            var links = new List<ILinkObject>();
            var embedded = new List<IEmbeddedResourceObject>();
            var state = new List<IStateValue>();

            ParseResourceObject(outer, links, embedded, state);

            return new RootResourceObject(links, embedded, state);
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
                            throw new FormatException("Invalid value for _links: " + value);
                        case "_embedded":
                            throw new FormatException("Invalid value for _embedded: " + value);
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

            foreach (var inner in outer.Properties())
            {
                var value = inner.Value.ToString();

                if (string.IsNullOrEmpty(value))
                    continue; // nothing to assign, just leave the default value ...

                switch (inner.Name.ToLowerInvariant())
                {
                    case "href":
                        link.Href = TryCreateUri(value, UriKind.RelativeOrAbsolute);
                        break;
                    case "templated":
                        link.Templated = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "type":
                        link.Type = value;
                        break;
                    case "deprication":
                        link.Deprecation = TryCreateUri(value, UriKind.Absolute);
                        break;
                    case "name":
                        link.Name = value;
                        break;
                    case "profile":
                        link.Profile = TryCreateUri(value, UriKind.Absolute);
                        break;
                    case "title":
                        link.Title = value;
                        break;
                    case "hreflang":
                        link.HrefLang = value;
                        break;
                }
            }

            return link;
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
                    yield return factory((JObject)inner.Value, rel);
            }
        }

        private static Uri TryCreateUri(string value, UriKind kind)
        {
            try
            {
                return new Uri(value, kind);
            }
            catch (UriFormatException)
            {
                return null;
            }
        }
    }
}
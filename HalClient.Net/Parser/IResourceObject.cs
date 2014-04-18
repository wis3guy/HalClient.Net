using System.Collections.Generic;

namespace HalClient.Net.Parser
{
    /// <summary>
    /// Represents a resource
    /// <see cref="http://tools.ietf.org/html/draft-kelly-json-hal-06#section-4"/>
    /// </summary>
    public interface IResourceObject
    {
        IReadOnlyDictionary<string, IStateValue> State { get; }
        IReadOnlyDictionary<string, IEnumerable<IEmbeddedResourceObject>> Embedded { get; }
        IReadOnlyDictionary<string, IEnumerable<ILinkObject>> Links { get; }
    }
}
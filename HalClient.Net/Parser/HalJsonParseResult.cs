using System;
using System.Collections.Generic;

namespace HalClient.Net.Parser
{
	public class HalJsonParseResult
	{
		public HalJsonParseResult(IEnumerable<ILinkObject> links, IEnumerable<IEmbeddedResourceObject> embeddedResources,
			IEnumerable<IStateValue> stateValues)
		{
			if (links == null)
				throw new ArgumentNullException(nameof(links));

			if (embeddedResources == null)
				throw new ArgumentNullException(nameof(embeddedResources));

			if (stateValues == null)
				throw new ArgumentNullException(nameof(stateValues));

			Links = links;
			EmbeddedResources = embeddedResources;
			StateValues = stateValues;
		}

		public IEnumerable<ILinkObject> Links { get; }
		public IEnumerable<IEmbeddedResourceObject> EmbeddedResources { get; }
		public IEnumerable<IStateValue> StateValues { get; }
	}
}
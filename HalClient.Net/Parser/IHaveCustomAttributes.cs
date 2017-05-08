using System.Collections.Generic;

namespace HalClient.Net.Parser
{
    public interface IHaveCustomAttributes
    {
        IDictionary<string, object> CustomAttributes { get; }
    }
}
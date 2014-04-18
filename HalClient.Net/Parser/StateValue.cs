using System;

namespace HalClient.Net.Parser
{
    internal class StateValue : IStateValue
    {
        public StateValue(string name, string value, string type)
        {
            if (string.IsNullOrEmpty(name)) 
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException("type");

            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Type { get; private set; }
    }
}
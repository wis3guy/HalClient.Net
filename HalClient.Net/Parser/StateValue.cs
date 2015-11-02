using System;

namespace HalClient.Net.Parser
{
	internal class StateValue : IStateValue
	{
		public StateValue(string name, string value, string type)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			if (string.IsNullOrEmpty(type))
				throw new ArgumentNullException(nameof(type));

			Name = name;
			Value = value;
			Type = type;
		}

		public string Name { get; }
		public string Value { get; }
		public string Type { get; }
	}
}
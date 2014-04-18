namespace HalClient.Net.Parser
{
    public interface IStateValue
    {
        string Name { get; }
        string Value { get; }
        string Type { get; }
    }
}
namespace HalClient.Net.Parser
{
    public interface IHalJsonParser
    {
        IRootResourceObject ParseResource(string json);
    }
}
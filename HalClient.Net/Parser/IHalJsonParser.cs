namespace HalClient.Net.Parser
{
	public interface IHalJsonParser
	{
		HalJsonParseResult Parse(string json);
	}
}
namespace HalClient.Net
{
	public enum ResponseParseBehavior
	{
		/// <summary>
		/// Only parse the response if a success status code is returned
		/// </summary>
		SuccessOnly,

		/// <summary>
		/// Always parse the received response, even if an error status code is returned
		/// </summary>
		Always
	}
}
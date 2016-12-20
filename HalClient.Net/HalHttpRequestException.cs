using System;

namespace HalClient.Net
{
	[Serializable]
	public class HalHttpRequestException : Exception
	{
		public HalHttpRequestException(IHalHttpResponseMessage message) : base(message.ReasonPhrase)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			ResponseMessage = message;
		}

		public IHalHttpResponseMessage ResponseMessage { get; }
	}
}
using System;

namespace HalClient.Net
{
	internal class CreatedHalHttpClient
	{
		public CreatedHalHttpClient(IHalHttpClient decorated, HalHttpClient wrapped)
		{
			if (decorated == null)
				throw new ArgumentNullException(nameof(decorated));

			if (wrapped == null)
				throw new ArgumentNullException(nameof(wrapped));

			Decorated = decorated;
			Wrapped = wrapped;
		}

		public IHalHttpClient Decorated { get; }
		public HalHttpClient Wrapped { get; }
	}
}
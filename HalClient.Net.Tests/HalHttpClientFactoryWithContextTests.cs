using System;
using System.Net.Http;
using HalClient.Net.Parser;
using Moq;
using Xunit;

namespace HalClient.Net.Tests
{
	public class HalHttpClientFactoryWithContextTests
	{
		private readonly IHalJsonParser _mockedHalJsonParser = new Mock<IHalJsonParser>().Object;
		private readonly string _stringContext = "just-some-string-value";
		private readonly TestContext _complexContext = new TestContext {Id = Guid.NewGuid()};
		
		[Fact]
		public void CreateClient_StringContext_CreatesAClient()
		{
			var sut = new HalHttpClientFactoryWithContext<string>(_mockedHalJsonParser);
			
			using (var client = sut.CreateClient(_stringContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_StringContext_WithHttpClient()
		{
			var sut = new HalHttpClientFactoryWithContext<string>(_mockedHalJsonParser);

			using (var httpClient = new HttpClient())
			using (var client = sut.CreateClient(httpClient, _stringContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_StringContext_PassesContextToConfigure()
		{
			var sut = new TestFactoryWrapper<string>(_mockedHalJsonParser)
			{
				OnConfigure = (config, context) => Assert.Same(_stringContext, context)
			};

			using (var client = sut.CreateClient(_stringContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_StringContext_PassesContextToDecorate()
		{
			var sut = new TestFactoryWrapper<string>(_mockedHalJsonParser)
			{
				OnDecorate = (original, context) =>
				{
					Assert.Same(_stringContext, context);
					return original;
				}
			};

			using (var client = sut.CreateClient(_stringContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_ComplexContext_CreatesAClient()
		{
			var sut = new HalHttpClientFactoryWithContext<TestContext>(_mockedHalJsonParser);

			using (var client = sut.CreateClient(_complexContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_ComplexContext_WithHttpClient()
		{
			var sut = new HalHttpClientFactoryWithContext<TestContext>(_mockedHalJsonParser);

			using (var httpClient = new HttpClient())
			using (var client = sut.CreateClient(httpClient, _complexContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_ComplexContext_PassesContextToConfigure()
		{
			var sut = new TestFactoryWrapper<TestContext>(_mockedHalJsonParser)
			{
				OnConfigure = (config, context) =>Assert.Same(_complexContext, context)
			};

			using (var client = sut.CreateClient(_complexContext))
			{
				Assert.NotNull(client);
			}
		}

		[Fact]
		public void CreateClient_ComplexContext_PassesContextToDecorate()
		{
			var sut = new TestFactoryWrapper<TestContext>(_mockedHalJsonParser)
			{
				OnDecorate = (original, context) =>
				{
					Assert.Same(_complexContext, context);
					return original;
				}
			};

			using (var client = sut.CreateClient(_complexContext))
			{
				Assert.NotNull(client);
			}
		}

		private class TestContext
		{
			public Guid Id { get; set; }
		}

		private class TestFactoryWrapper<T> : HalHttpClientFactoryWithContext<T>
		{
			public TestFactoryWrapper(IHalJsonParser parser) : base(parser)
			{
			}

			public Action<IHalHttpClientConfiguration, T> OnConfigure { get; set; }

			protected override void Configure(IHalHttpClientConfiguration config, T context)
			{
				if (OnConfigure != null)
					OnConfigure(config, context);
				else
					base.Configure(config, context);
			}

			public Func<IHalHttpClient, T, IHalHttpClient> OnDecorate { get; set; }

			protected override IHalHttpClient Decorate(IHalHttpClient original, T context)
			{
				return OnDecorate != null 
					? OnDecorate(original, context) 
					: base.Decorate(original, context);
			}
		}
	}
}
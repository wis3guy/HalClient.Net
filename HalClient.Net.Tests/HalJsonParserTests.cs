using System.Globalization;
using System.Linq;
using HalClient.Net.Parser;
using Xunit;

namespace HalClient.Net.Tests
{
	public class HalJsonParserTests
	{
		/// <summary>
		///     Example document, taken from: http://stateless.co/hal_specification.html
		/// </summary>
		private const string Json = @"
{
	""_links"": {
		""self"": { ""href"": ""/orders"" },
		""curies"": [{ ""name"": ""ea"", ""href"": ""http://example.com/docs/rels/{rel}"", ""templated"": true }],
		""next"": { ""href"": ""/orders?page=2"" },
		""ea:find"": {
			""href"": ""/orders{?id}"",
			""templated"": true
		},
		""ea:admin"": [{
			""href"": ""/admins/2"",
			""title"": ""Fred""
		}, {
			""href"": ""/admins/5"",
			""title"": ""Kate""
		}]
	},
	""currentlyProcessing"": 14,
	""shippedToday"": 20,
	""_embedded"": {
		""ea:order"": [{
			""_links"": {
				""self"": { ""href"": ""/orders/123"" },
				""ea:basket"": { ""href"": ""/baskets/98712"" },
				""ea:customer"": { ""href"": ""/customers/7809"" }
			},
			""total"": 30.00,
			""currency"": ""USD"",
			""status"": ""shipped""
		}, {
			""_links"": {
				""self"": { ""href"": ""/orders/124"" },
				""ea:basket"": { ""href"": ""/baskets/97213"" },
				""ea:customer"": { ""href"": ""/customers/12369"" }
			},
			""total"": 20.00,
			""currency"": ""USD"",
			""status"": ""processing""
		}]
	}
}";

		private const string JsonNullEmbedded = @"
{
	""_links"": {
		""self"": { ""href"": ""/orders"" },
		""curies"": [{ ""name"": ""ea"", ""href"": ""http://example.com/docs/rels/{rel}"", ""templated"": true }],
		""next"": { ""href"": ""/orders?page=2"" },
		""ea:find"": {
			""href"": ""/orders{?id}"",
			""templated"": true
		},
		""ea:admin"": [{
			""href"": ""/admins/2"",
			""title"": ""Fred""
		}, {
			""href"": ""/admins/5"",
			""title"": ""Kate""
		}]
	},
	""currentlyProcessing"": 14,
	""shippedToday"": 20,
	""_embedded"": null
}";

		private const string JsonNullLinks = @"
{
	""_links"": null,
	""currentlyProcessing"": 14,
	""shippedToday"": 20,
	""_embedded"": {
		""ea:order"": [{
			""_links"": {
				""self"": { ""href"": ""/orders/123"" },
				""ea:basket"": { ""href"": ""/baskets/98712"" },
				""ea:customer"": { ""href"": ""/customers/7809"" }
			},
			""total"": 30.00,
			""currency"": ""USD"",
			""status"": ""shipped""
		}, {
			""_links"": {
				""self"": { ""href"": ""/orders/124"" },
				""ea:basket"": { ""href"": ""/baskets/97213"" },
				""ea:customer"": { ""href"": ""/customers/12369"" }
			},
			""total"": 20.00,
			""currency"": ""USD"",
			""status"": ""processing""
		}]
	}
}";

		private readonly HalJsonParser _sut = new HalJsonParser();

		[Fact]
		public void EmbeddedParsing_ParsesTheCorrectNumberOfRels()
		{
			var result = _sut.Parse(Json);

			Assert.Equal(1, result.EmbeddedResources.Select(x => x.Rel).Distinct().Count());
		}

		[Fact]
		public void EmbeddedParsing_ParsesTheCorrectRels()
		{
			var result = _sut.Parse(Json);

			Assert.True(result.EmbeddedResources.All(x => x.Rel == "ea:order"));
		}

		[Fact]
		public void EmbeddedParsing_ParsesCorrectNumberOfEmbeddedResourceObjects()
		{
			var result = _sut.Parse(Json);

			Assert.Equal(2, result.EmbeddedResources.Count());
		}

		[Fact]
		public void EmbeddedParsing_StateParsing_ParsesCorrectNumberOfStateValues()
		{
			var result = _sut.Parse(Json);
			var order = result.EmbeddedResources.First(x => x.Rel == "ea:order");

			Assert.Equal(3, order.State.Count);
		}

		[Fact]
		public void EmbeddedParsing_StateParsing_ParsesCorrectStateValues()
		{
			var result = _sut.Parse(Json);
			var order = result.EmbeddedResources.First(x => x.Rel == "ea:order");

			Assert.Equal((30.00f).ToString(CultureInfo.CurrentCulture), order.State["total"].Value);
			Assert.Equal("USD", order.State["currency"].Value);
			Assert.Equal("shipped", order.State["status"].Value);
		}

		[Fact]
		public void EmbeddedParsing_ParsesNullEmbedded()
		{
			var result = _sut.Parse(JsonNullEmbedded);

			Assert.Equal(5, result.Links.Select(x => x.Rel).Distinct().Count());
		}

		[Fact]
		public void StateParsing_ParsesCorrectNumberOfStateValues()
		{
			var result = _sut.Parse(Json);

			Assert.Equal(2, result.StateValues.Count());
		}

		[Fact]
		public void StateParsing_ParsesCorrectStateValues()
		{
			var result = _sut.Parse(Json);

			Assert.Equal("14", result.StateValues.Single(x => x.Name == "currentlyProcessing").Value);
			Assert.Equal("20", result.StateValues.Single(x => x.Name == "shippedToday").Value);
		}

		[Fact]
		public void LinkParsing_ParsesCorrectNumberOfRels()
		{
			var result = _sut.Parse(Json);

			Assert.Equal(5, result.Links.Select(x => x.Rel).Distinct().Count());
		}

		[Fact]
		public void LinkParsing_ParsesCorrectNumberOfLinkObjects()
		{
			var result = _sut.Parse(Json);

			Assert.Equal(6, result.Links.Count());
		}

		[Fact]
		public void LinkParsing_ParsesCorrectRels()
		{
			var result = _sut.Parse(Json);

			Assert.True(result.Links.Any(x => x.Rel == "self"));
			Assert.True(result.Links.Any(x => x.Rel == "curies"));
			Assert.True(result.Links.Any(x => x.Rel == "next"));
			Assert.True(result.Links.Any(x => x.Rel == "ea:find"));
			Assert.True(result.Links.Any(x => x.Rel == "ea:admin"));
		}

		[Fact]
		public void LinkParsing_ParsesCorrectSelfLink()
		{
			var result = _sut.Parse(Json);

			Assert.Equal("/orders", result.Links.Single(x => x.Rel == "self").Href.ToString());
		}

		[Fact]
		public void LinkParsing_ParsesCorrectAdminTitles()
		{
			var result = _sut.Parse(Json);

			Assert.True(result.Links.Where(x => x.Rel == "ea:admin").Any(x => x.Title.Equals("Kate")));
			Assert.True(result.Links.Where(x => x.Rel == "ea:admin").Any(x => x.Title.Equals("Fred")));
		}

		[Fact]
		public void LinksParsing_ParsesNullLinks()
		{
			var result = _sut.Parse(JsonNullLinks);

			Assert.Equal(2, result.EmbeddedResources.Count());
		}
		
		/// <summary>
		/// See: https://github.com/wis3guy/HalClient.Net/issues/27
		/// </summary>
		[Fact]
		public void StateParsing_LeavesDatesUntouched()
		{
			const string expected = "2019-04-22T20:52:50Z";
			
			var json = $"{{'date': '{expected}'}}";
			var parser = new HalJsonParser();
			var result = parser.Parse(json);
			var date = result.StateValues.Single(sv => sv.Name == "date");

			Assert.Equal(date.Value, expected);
		}
	}
}
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

        private readonly HalJsonParser _parser = new HalJsonParser();

        [Fact]
        public void LinkParsing_CanObtainDocumentationUri()
        {
            var resource = _parser.ParseResource(Json);
            var uri = resource.GetDocumentationUri(resource.Embedded["ea:order"].First());

            Assert.NotNull(uri);
            Assert.Equal("http://example.com/docs/rels/order", uri.ToString());
        }

        [Fact]
        public void EmbeddedParsing_ParsesTheCorrectNumberOfRels()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal(1, resource.Embedded.Count);
        }

        [Fact]
        public void EmbeddedParsing_ParsesTheCorrectRels()
        {
            var resource = _parser.ParseResource(Json);

            Assert.True(resource.Embedded.ContainsKey("ea:order"));
        }

        [Fact]
        public void EmbeddedParsing_ParsesCorrectNumberOfEmbeddedResourceObjects()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal(2, resource.Embedded.SelectMany(x => x.Value).Count());
        }

        [Fact]
        public void EmbeddedParsing_StateParsing_ParsesCorrectNumberOfStateValues()
        {
            var resource = _parser.ParseResource(Json);
            var order = resource.Embedded["ea:order"].First();

            Assert.Equal(3, order.State.Count);
        }

        [Fact]
        public void EmbeddedParsing_StateParsing_ParsesCorrectStateValues()
        {
            var resource = _parser.ParseResource(Json);
            var order = resource.Embedded["ea:order"].First();

            Assert.Equal((30.00f).ToString(CultureInfo.CurrentCulture), order.State["total"].Value);
            Assert.Equal("USD", order.State["currency"].Value);
            Assert.Equal("shipped", order.State["status"].Value);
        }

        [Fact]
        public void StateParsing_ParsesCorrectNumberOfStateValues()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal(2, resource.State.Count);
        }

        [Fact]
        public void StateParsing_ParsesCorrectStateValues()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal("14", resource.State["currentlyProcessing"].Value);
            Assert.Equal("20", resource.State["shippedToday"].Value);
        }

        [Fact]
        public void LinkParsing_ParsesCorrectNumberOfRels()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal(5, resource.Links.Count);
        }

        [Fact]
        public void LinkParsing_ParsesCorrectNumberOfLinkObjects()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal(6, resource.Links.SelectMany(x => x.Value).Count());
        }

        [Fact]
        public void LinkParsing_ParsesCorrectRels()
        {
            var resource = _parser.ParseResource(Json);

            Assert.True(resource.Links.ContainsKey("self"));
            Assert.True(resource.Links.ContainsKey("curies"));
            Assert.True(resource.Links.ContainsKey("next"));
            Assert.True(resource.Links.ContainsKey("ea:find"));
            Assert.True(resource.Links.ContainsKey("ea:admin"));
        }

        [Fact]
        public void LinkParsing_ParsesCorrectSelfLink()
        {
            var resource = _parser.ParseResource(Json);

            Assert.Equal("/orders", resource.Links["self"].Single().Href.ToString());
        }

        [Fact]
        public void LinkParsing_ParsesCorrectAdminTitles()
        {
            var resource = _parser.ParseResource(Json);

            Assert.True(resource.Links["ea:admin"].Any(x => x.Title.Equals("Kate")));
            Assert.True(resource.Links["ea:admin"].Any(x => x.Title.Equals("Fred")));
        }
    }
}
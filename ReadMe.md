#HalClient.Net
A specialized http client  that simplifies communicating with API's that support the HAL media type.

##HAL Specification resources
* https://github.com/mikekelly/hal_specification
* https://tools.ietf.org/html/draft-kelly-json-hal-06

##Basic usage
At application startup (ie. `Main()` or `Application_Start()`) setup the `ApiHalClientFactory`:

```c#
// Create the default parser
IHalJsonParser parser = new HalJsonParser();

// Create the factory
IHalHttpClientFactory factory = new HalHttpClientFactory(parser);
```

The factory is now ready for use and can be held, in your IoC container of choice, within singleton scope.

Within your code, you can now use the factory to create your clients as follows:

```c#
using (var client = factory.CreateClient())
{
    var resource = await client.GetAsync(new Uri("http://example.com/orders"));

    // get the self link of the root resource
    var selfUri = resource.Links["self"].First().Href;
    
    // get the order dates of all embedded order resources
    var orderDates = resource.Embedded["example:order"].Select(x => x.State["Date"].Value);
    
    // automatically resolve the documentation uri for a named link relation based on curies
    var documentationUri = resource.GetDocumentationUri(resource.Links["address:invoice"].First());
}
```

Note that the client is disposable!

The object graph returned by the client is a generic representation of the object, that makes contained data elements easy to traverse/access. Given the flexibility of JSON and the pace at which API's change these days, this strategy was chosen over ORM-like object mapping. You could (relatively) easily build such behavior into a wrapper class though.

##Creating a custom `HalClientFactory`
By creating a custom factory, you are able to override the `Configure(IHalClientConfiguration config)`. This makes it possible to consistently apply the same configuration to all created `IHalClient` instances.

```c#
public class CustomApiCLientFactory : HalHttpClientFactory
{
    private readonly string _apiKey;

    public CustomApiCLientFactory(IHalJsonParser parser, string apiKey) : base(parser)
    {
        if (string.IsNullOrEmpty(apiKey)) 
            throw new ArgumentNullException("apiKey");

        _apiKey = apiKey;
    }

    protected override void Configure(IHalHttpClientConfiguration config)
    {
        config.BaseAddress = new Uri("http://example.com");
        config.Headers.Add("Authorization",string.Format("API_KEY_SCHEME apikey=\"{0}\"", _apiKey));
        config.MaxResponseContentBufferSize = 1024;
        config.Timeout = TimeSpan.FromSeconds(10);
        config.ApiRootResourceCachingBehavior = CachingBehavior.Once;
        config.ParseBehavior = ResponseParseBehavior.Always
    }
}
```
The following options can be configured:

 Setting | Description
:--|:--
`BaseAddress` | Exposes the `BaseAddress` property of the underlying `HttpClient` instance.
`Headers` | Exposes the `Headers` collection of the underlying `HttpClient` instance.
`MaxResponseContentBufferSize` | Exposes the `MaxResponseContentBufferSize` property of the underlying `HttpClient` instance.
`Timeout` | Exposes the `Timeout ` property of the underlying `HttpClient` instance.
`ApiRootResourceCachingBehavior ` | Tells the `HalClientFactory` wether or not the API's root resource should be cached.
`ParseBehavior` | Tells the `IHalClient` instance wether or not error responses should be parsed.

###ApiRootResourceCachingBehavior
Most HAL based API's expose their entry points as links in the root response. In order to embrace this paradigm, it is possible to have the `HalHttpClientFactory` retrieve and cache the root response for future reference.

Possible options for this property are:

Value | Description
:--|:--
`Never` | The API's root resource will not be automatically retrieved nor cached.
`PerClient` | The API's root resource will be automatically retrieved and cached every time a `IHalClient` instance is created and will remain cached as long as the client is not disposed.
`Once` | The API's root resource will be automatically retrieved and cached when the first `IHalClient` instance is created and will remain cached as long as the `HalClientFactory` is not garbage collected.

Note that caching the API's root resource can reduce chatter, as this resource typically returns all hyperlinks needed to navigate the API. When interacting with an API that returns different hyperlinks in the root resource, based on authorization, opt to cache per client, otherwise cache once.

###ParseBehavior
Possible options for this property are:

Value | Description
:--|:--
`Always` | Parse the response from the API, regardless the HTTP status code.
`SuccessOnly` | Only parse the response from the API in case of a success HTTP status code.

Note that an exception will be thrown in case the `Content-Type` of the response is not `application/hal+json` and the client is tries to parse the response.

Setting the value to `Always` is only useful when communicating with API's that return error description in the HAL mediatype format.

##Using a custom `HttpClient`
If you require a custom `HttpClient` to be wrapped by the `IHalClient` instance, you can provide it to the factory method, like so:

```c#
var parser = new HalJsonParser();
var factory = new HalHttpClientFactory(parser);
var custom = new HttpClient(); // This would be your own http client

using (var client = factory.CreateClient(custom))
{
    var uri = client.Root.Links["example:orders"].First().Href;
    var resource = await client.GetAsync(uri);

    // do useful things with the response ...
}
```

This feature is also useful in test-scenarios where you might want to use a different http client that in production.

##Work in progress
This library has spawned from an adhoc need i had to communicate with one of my own API's. As such it has been developed up until the point where it met my particular needs. It may not suit all of your use cases. If so, feel free to file an Issue or even a Pull Request.

##Credits
The parser makes use of Darrel Miller's UriTemplates project: https://github.com/tavis-software/UriTemplates

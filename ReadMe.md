HalClient.Net
==========

A specialized http client  that simplifies communicating with API's that support the HAL media type.

HAL Specification
-----------------
https://github.com/mikekelly/hal_specification
https://tools.ietf.org/html/draft-kelly-json-hal-06

Basic usage
-----------
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

The object graph returned by the client is a generic representation of the object, that makes contained data elements easy to traverse/access. Given the flexibility of JSON and the pace at which API's change these days, this strategy was chosen over ORM-like object mapping. You could (relatively) easily build that in a wrapper class specific in your own codebase though.

Caching the root response
-------------------------
Most HAL based API's expose their entry points as links in the root response. In order to embrace this paradigm, it is possible to have the `HalHttpClientFactory` retrieve and cache the root response for future reference. The factory will the set the root response on each created client so it is easily accessible to, for example, extension methods.

```c#
//
// At application start

IHalHttpClientWithRootFactory factory = new HalHttpClientFactory(new HalJsonParser());

//
// In your consuming code

using (var client = factory.CreateClientWithRoot(new Uri("http://api.example.com"))
{
    var uri = client.Root.Links["example:orders"].First().Href;
    var resource = await client.GetAsync(uri);

    // do useful things with the response ...
}
```

More control
------------
If you frequently need to interact with a specific API, specifying full uri's can become a pain and especially error prone. Also, you might want to have some more control over the way the client behaves. In such cases you can create a specific factory:

```c#
public class SpecificApiCLientFactory : HalHttpClientFactory
{
    private readonly string _apiKey;

    public SpecificApiCLientFactory(IHalJsonParser parser, string apiKey) : base(parser)
    {
        if (string.IsNullOrEmpty(apiKey)) 
            throw new ArgumentNullException("apiKey");

        _apiKey = apiKey;
    }

    protected override void Configure(IHalHttpClientConfiguration client)
    {
        client.BaseAddress = new Uri("http://example.com");
        client.Headers.Add("Authorization",string.Format("API_KEY_SCHEME apikey=\"{0}\"", _apiKey));
        client.MaxResponseContentBufferSize = 1024;
        client.Timeout = TimeSpan.FromSeconds(10);
    }
}
```

In case you set the client's `BaseAddress` property in the `Configure()` method of your factory, use the overload of `CreateClientWithRoot()` which does not require the base address as a parameter. 

Work in progress
----------------
This library has spawned from an adhoc need i had to communicate with one of y own API's. As such it has been developed up until the point where it met my particular needs. It may not suit all of your use cases. If so, feel free to file an Issue or even a Pull Request.

Credits
-------
The parser makes use of Darrel Miller's UriTemplates project: https://github.com/tavis-software/UriTemplates

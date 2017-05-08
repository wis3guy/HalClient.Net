# HalClient.Net 
A specialised http client  that simplifies communicating with API's that support the HAL media type.

[![Build status](https://ci.appveyor.com/api/projects/status/klqo0binme3x9k3b/branch/master?svg=true)](https://ci.appveyor.com/project/wis3guy/halclient-net/branch/master)

## HAL Specification resources
* [hal_specification](https://github.com/mikekelly/hal_specification)
* [draft-kelly-json-hal-06](https://tools.ietf.org/html/draft-kelly-json-hal-06)

## Why a specialised `HttpClient`
Dealing with the responses from a HAL enabled API can be tedious. As a consumer you repeatedly need to extract the links, embedded resources and state values to be able to reason about these entities. This client aims to be a thin wrapper around the `HttpClient` that takes care of the initial parsing of HAL responses. In most cases, consuming code does not need to do any parsing so it can focus on interpreting.

The object graph returned by the client is a generic representation of the object, which makes contained data elements easy to traverse/access. Given the flexibility of JSON and the pace at which API's change these days, this strategy was chosen over ORM-like object mapping. 

> Using this library as a starting point, you could (relatively) easily build such behaviour into a wrapper class though.

## Basic usage
At application startup (ie. `Main()` or `Application_Start()`) setup the `ApiHalHttpClientFactory`:

```c#
// Create the default parser
IHalJsonParser parser = new HalJsonParser();

// Create the factory
IHalHttpClientFactory factory = new HalHttpClientFactory(parser);
```

The factory is now ready for use and can be held, in your IoC container of choice, in singleton scope.

Within your code, you can now use the factory to create your clients as follows:

```c#
using (var client = factory.CreateClient())
{
	using(var response = await client.GetAsync(new Uri("http://example.com/orders")))
	{
		if (response.IsHalResponse)
		{
			// get the self link of the root resource
			var selfUri = response.Resource.Links["self"].First().Href;
	
			// get the order dates of all embedded order resources
			var orderDates = response.Resource.Embedded["example:order"].Select(x => x.State["Date"].Value);
	
			// automatically resolve the documentation uri for a named link relation based on curies
			var documentationUri = response.Resource.GetDocumentationUri(resource.Links["address:invoice"].First());
		}
	}
}
```

> Note that both the client and the response are disposable!

Depending on how you set the `ThrowOnError` (default value is `true)`flag in the configuration, be sure to check the `response.Message.IsSuccessStatusCode` before deciding what to do with the returned information.

### Working with `ILinkObject` instances
After parsing the `application/hal+json` response from the API, the returned resource contains a dictionary of links as they were encounteres in the `_links` property of the resource. The key in this dictionary is the link's `rel`, the value of each pair in the dictionary, is an `IEnumerable<ILinkObject>`. The reason for this being an enumerable is that the response might contain multiple links with the same `rel` attribute.

Each `ILinkObject` instance represents a link, as defined in [RFC5988](https://tools.ietf.org/html/rfc5988). Note that as specified in the [hal spec](https://tools.ietf.org/html/draft-kelly-json-hal-06), links may be provided as URI templates as defined in [RFC6570](http://tools.ietf.org/html/rfc6570). `ILinkObject` instances can subsequently represent templated or non templated links. In case the link is templated, the `Href` property will be `null` and the `Template` property will represent the template. Alternatively, the `Href` will be set to a vilud `Uri` and the `Template` will be `null`. Below is the recommended way of working with links.

```c#
//
// In case of embedded resources, you might have many links with the same rel ...
//

var productLinks = resource.Links["product"]; // list of links to all embedded products

Assert.IsTrue(productLinks.Any());

//
// A self link is never templated, and there is only 1 ...
//

var selfLink = resource.Links["self"].Single();

Assert.IsFalse(selfLink.Templated);
Assert.IsNull(selfLink.Template);
Assert.IsNotNull(selfLink.Href);

//
// Here we can export a resource in various formats
//

var exportLink = resource.Links["export"].Single();

Assert.IsTrue(exportLink.Templated);
Assert.IsNotNull(exportLink.Template);
Assert.IsNull(exportLink.Href);

var resolved = exportLink.ResolveTemplated(x => x.AddParameter("format", "xml").Resolve());

Assert.IsFalse(resolved.Templated);
Assert.IsNull(resolved.Template);
Assert.IsNotNull(resolved.Href);
```
Note that the `ResolveTemplated` returns a new `ILinkObject` instance, which is no longer templated. Although it is possible to instruct the `UriTemplate` instance, passed into the `Func<UriTemplate, string>` you **should not** do partial parameter replacements as the resulting `ILinkObject` will no longer allow you to replace other parameters.

For more info on how to use the `UriTemplate` class, please visit: [Tavis.UriTemplates](https://github.com/tavis-software/Tavis.UriTemplates)

## Advanced usage
There are many ways you could customise the behaviour of `IHalHttpClient` instances and of the `HalHttpClientFactory`. Below is a list of scenarios and recommended approaches.

#### I want to cache the API root resource
Many hypermedia powered API's expose their entry points as links in the root response. In order to embrace this paradigm, and reduce chatter, it is possible to have the `HalHttpClientFactory` retrieve and cache the root response for future reference.

Possible `CachingBehavior` options are:

Value | Description
:--|:--
`Never` | The API's root resource will not be automatically retrieved nor cached.
`PerClient` | The API's root resource will be automatically retrieved and cached every time a `IHalHttpClient` instance is created and will remain cached as long as the client is not disposed.
`Once` | The API's root resource will be automatically retrieved and cached when the first `IHalHttpClient` instance is created and will remain cached as long as the `HalHttpClientFactory` is not garbage collected.

In order to make use of the built-in caching mechanism, use one of the awaitable `CreateClientAsync()` overloads.

```c#
using (var client = await factory.CreateClientAsync(CachingBehavior.PerClient))
{
	// client.CachedApiRootResource is set to a parsed IRootResourceObject instance
}
```

## I want to access a non-HAL resource
In some situations, an HAL media type API may respond using a non-HAL content type for certain resources. Consider the download of an image or other binary. In these cases, you should use the `IHalClient.HttpClient` to do the communication.

### I want to apply common configuration to all `IHalHttpClient` instances
If you want to configure all instantiated `IHalHttpClient` objects consistently, you should create a custom factory.

```c#
public class CustomApiCLientFactory : HalHttpClientFactory
{
	private readonly string _apiKey;

	public CustomApiCLientFactory(IHalJsonParser parser, string apiKey) : base(parser)
	{
		_apiKey = apiKey;
	}

	protected override void Configure(IHalHttpClientConfiguration config)
	{
		config.BaseAddress = new Uri("http://example.com");
		config.Headers.Add("Authorization",string.Format("API_KEY_SCHEME apikey=\"{0}\"", _apiKey));
		config.MaxResponseContentBufferSize = 1024;
		config.Timeout = TimeSpan.FromSeconds(10);
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
`ThrowOnError` | Wether a `HalHttpRequestException` should be thrown upon receiving a non-success response from the server. The default value is true.
`AutoFollowRedirects` | Wether the client should automatically follow (ie. perform a subsequent GET request) in case the server returns either an HTTP 302 or 303 status code. The default value is true.

> Note that, whatever `Accept` header you configure, the value will be overridden and set to `application/hal+json` unless you communicate using the `IHalHttpClient.HttpClient`.

### I want to wrap all 'IHalHttpClient` instances in a custom object

Sometimes it might be useful to wrap all instantiated `IHalHttpClient` in a custom object. This is typically useful when you want to layer some logic on top of the provided behaviour. In such cases you should create a [decorator](https://en.wikipedia.org/wiki/Decorator_pattern) for the `IHalHttpClient` as well as a custom `HalHttpClientFactory`.

```c#
public class CustomHalHttpClient : IHalHttpClient
{
	IHalHttpClient _decorated;

	public CustomHalHttpClient(IHalHttpClient decorated)
	{
		_decorated = decorated;
	}

	public Task<IHalHttpResponseMessage> PostAsync<T>(Uri uri, T data)
	{
		//
		// do something fancy with the uri and/or data
		//

		var resource = _decorated.PostAsync(uri, data);

		//
		// do something fancy with the result
		//
	}

	public Task<IHalHttpResponseMessage> PutAsync<T>(Uri uri, T data)
	{
		//
		// Custom code might go here
		//
	}

	public Task<IHalHttpResponseMessage> GetAsync(Uri uri)
	{
		//
		// Custom code might go here
		//
	}

	public Task<IHalHttpResponseMessage> DeleteAsync(Uri uri)
	{
		//
		// Custom code might go here
		//
	}

	public Task<IHalHttpResponseMessage> SendAsync(HttpRequestMessage request)
	{
		//
		// Custom code might go here
		//
	}

	public IRootResourceObject CachedApiRootResource => _decorated.CachedApiRootResource;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
			return;

		if (_decorated == null)
			return;

		_decorated.Dispose();
		_decorated = null;
	}
}

public class CustomHalHttpClientFactory : HalHttpClientFactory
{
	public CustomHalHttpClientFactory(IHalJsonParser parser) : base(parser)
	{
	}

	protected override IHalHttpClient Decorate(IHalHttpClient original)
	{
		return new CustomHalHttpClient(original);
	}
}
```

### I want to pass an adhoc context object to my `Configure` and/or `Decorate` overrides
There are scenarios where you might need to pass a context object from the code calling `HalHttpClientFactory.Create()` to the custom `Configure` and/or `Decorate` overrides of you custom factory. This is typically useful when dealing with remote use impersonation, where your client makes API requests on behalf of a remote user.

To help you deal with such situations, there is an abstract generic `HalHttpClientFactoryBase<T>` class from which you can derive your custom factory.

Note that the abstract generic `HalHttpClientFactoryBase<T>` class derives from the `HalHttpClientFactory` class, and thus allows for the same overrides.

```c#
public class CustomHalHttpClientFactory : HalHttpClientFactoryBase<string>
{
	public CustomHalHttpClientFactory(IHalJsonParser parser) : base(parser)
	{
	}

	protected override void Configure(IHalHttpClientConfiguration config)
	{
		//
		// Custom Configure, in case a context was *not* specified in the CreateClient() call
		//
	}

	protected override void Configure(IHalHttpClientConfiguration config, string context)
	{
		//
		// Custom Configure, in case a context was specified in the CreateClient() call
		//
	}

	protected override IHalHttpClient Decorate(IHalHttpClient original)
	{
		//
		// Custom Decorate, in case a context was *not* specified in the CreateClient() call
		//
	}

	protected override IHalHttpClient Decorate(IHalHttpClient original, string context)
	{
		//
		// Custom Decorate, in case a context was specified in the CreateClient() call
		//
	}
}
```

### I want to use a custom `HttpClient` for my `IHalHttpClient` instances
In case this an adhoc need, ie. a *differently initialised* `HttpClient` is needed for each instantiated `IHalHttpClient`, simply use the appropriate overload of the `HalHttpClientFactory.Create()` method.

```c#
var custom = new HttpClient();

//
// Configure the custom HttpClient instance
//

using (var client = factory.CreateClient(custom))
{
	//
	// Do something with the client ...
	//
}
```

In case this an constant need, ie. a *consistently initialised* `HttpClient` is needed for each instantiated `IHalHttpClient`, you should create a custom factory, and override the `IHalHttpClientFactory.GetHttpClient()` method.

```c#
public sealed class CustomHalHttpClientFactory : HalHttpClientFactory
{
	private readonly long _apiClientId;
	private readonly string _secretKey;

	public CustomHalHttpClientFactory(long apiClientId, string secretKey)
	{
		_apiClientId = apiClientId;
		_secretKey = secretKey;
	}	
	
	protected override HttpClient GetHttpClient()
	{
		var custom = new HttpClient();
		
		//
		// Configure the custom HttpClient instance
		//
				
		return new HttpClient(custom);
	}
}
```

### I want to use a custom `HttpMessageHandler` for my `IHalHttpClient` instances
In case this an adhoc need, ie. a *differently initialised* `HttpMessageHandler ` is needed for each instantiated `IHalHttpClient`, simply use the appropriate overload of the `HalHttpClientFactory.Create()` method. This is typically useful for testing purposes

```c#
public class FakeResponseHandler : DelegatingHandler
{
	private readonly Dictionary<Uri, HttpResponseMessage> _fakeResponses = new Dictionary<Uri, HttpResponseMessage>(); 

	public void AddFakeResponse(Uri uri, HttpResponseMessage responseMessage)
	{
		_fakeResponses.Add(uri,responseMessage);
	}

	protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
	{
		if (_fakeResponses.ContainsKey(request.RequestUri))
		{
			return _fakeResponses[request.RequestUri];
		}
		else
		{
			return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request};
		}
	}
}

var custom = new FakeResponseHandler();

custom.AddFakeResponse(new Uri("http://example.org/test"), new HttpResponseMessage(HttpStatusCode.OK));

using (var client = factory.CreateClient(custom))
{
	//
	// Do something with the client ...
	//
}
```

> Code snippet for the `FakeResponseHandler` taken from [stackoverflow.com](http://stackoverflow.com/a/22264503)

In case this an constant need, ie. a *consistently initialised* `HttpMessageHandler ` is needed for each instantiated `IHalHttpClient`, you should create a custom factory, and override the `IHalHttpClientFactory.GetHttpClient()` method. This is typically useful for message signing scenarios.

```c#
internal class CustomHttpMessageHandler : HttpClientHandler
{
	private const string AuthenticationScheme = "CustomScheme";

	private readonly string _secretKey;
	private readonly long _apiClientId;

	public CustomHttpMessageHandler(long apiClientId, string secretKey)
	{
		_secretKey = secretKey;
		_apiClientId = apiClientId;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		//
		// Calculate HMAC hash and set the appropriate headers on the request
		//

		return base.SendAsync(request, cancellationToken);
	}
}

public sealed class CustomHalHttpClientFactory : HalHttpClientFactory
{
	private readonly long _apiClientId;
	private readonly string _secretKey;

	public CustomHalHttpClientFactory(long apiClientId, string secretKey)
	{
		_apiClientId = apiClientId;
		_secretKey = secretKey;
	}	
	
	protected override HttpClient GetHttpClient()
	{
		return new HttpClient(new CustomHttpMessageHandler(_apiClientId, _secretKey));
	}
}
```

## Thread-safety
* `IHalHttpClientFactory` instances are thread-safe;
* `IHalHttpClient` instances are **not** thread-safe;

## Error handling
Handling errors, when using a `IHalHttpClient` is no different from when using a regular `HttpClient`. Given the asynchronous nature, you should catch `AggregateException` and deal with the inner exceptions.

There is one major difference in the type of exceptions that might be thrown. Rather than throwing a `HttpRequestException` the client might throw a `HalHttpRequestException`. Reason for this custom is that even an error response from the API might be a valid HAL resource. If so -- based on the `Content-Type` header of the response -- the response will be parsed and made available as a property on the exception for further interpretation.

```c#
try
{
	using (var client = factory.CreateClient())
	{
		//
		// Do something with the client ...
		//
	}
}
catch (AggregateException aggregate)
{
	aggregate.Handle(e =>
	{
		var hal = e as HalHttpRequestException;

		if (hal != null)
		{
			var statusCode = hal.StatusCode; // response status code
			var resource = hal.Resource; // error response (might be null)

			//
			// Deal with the error ...
			//
			
			return true;
		}
		
		return false;
	});	
}
catch(HalHttpRequestException e)
{
	//
	// Deal with the error ...
	//
}
```

## Nuget
A nuget package for this library is available here: https://www.nuget.org/packages/HalClient.Net/

## Work in progress
This library has spawned from an adhoc need i had to communicate with one of my own API's, which uses (WebApi.Hal)[https://github.com/JakeGinnivan/WebApi.Hal]. As time progressed, my experience with the mediatype grew and my needs changed. this has led to many small and big changes to the library.

As with any library, it may not suit all of your use cases. I am very much interested in your particular use cases and am eager to improve the library. Feel free to create an Issue or (even better) a Pull Request. 

## Credits
The parser makes use of Darrel Miller's UriTemplates project: https://github.com/tavis-software/Tavis.UriTemplates

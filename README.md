# Azure Functions HTTP Extensions

[![Wiki](https://img.shields.io/badge/docs-in%20wiki-green.svg?style=flat)](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions/wiki) 
[![Wiki](https://img.shields.io/nuget/v/AzureFunctionsV2.HttpExtensions.svg)](https://www.nuget.org/packages/AzureFunctionsV2.HttpExtensions/) 

![Logo](assets/logo.png)


This C# library extends the Azure Functions HTTP Trigger and adds useful extensions to 
make working with HTTP requests more fluent. It allows you to
add HTTP parameters from headers, query parameters, body and form fields directly
to the function signature. It also adds some boilerplate code to take advantage of
Function Filters in v2 Functions, allowing you to specify cross-cutting Exception 
handling tasks combined with an overridable error response formatter.

## Features

- Enables you to define Function signatures similar to ASP.NET Controller conventions
- Automatically deserializes objects, lists and arrays, supporting JSON and XML content out of the box
- Provides basic input validation via JSON deserializer
- Provides basic JWT-based authentication/authorization via attributes, which is also customizable
- Provides an exception filter, allowing more control over responses
- Allows overriding default deserialization methods and exception handling with custom behaviour

### Example usage

```C#
[HttpJwtAuthorize]
[FunctionName("TestFunction")]
public static async Task<IActionResult> TestFunction (
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mymethod/{somestring}")] HttpRequest req,
    string somestring,
    [HttpQuery(Required = true)]HttpParam<string> stringParam,
    [HttpQuery]HttpParam<List<int>> intParams,
    [HttpQuery]HttpParam<TestEnum> enumParam,
    [HttpBody]HttpParam<MyClass> myBodyObject,
    [HttpHeader(Name = "x-my-header")]HttpParam<string> customHeader,
    [HttpJwt]HttpUser user,
    ILogger log)
{
	string blah = stringParam; // implicit operator

	foreach(var x in intParams.Value) {
		// ...
	}

    return new OkObjectResult("");
}
```

See the [wiki](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions/wiki) for details on 
the attributes and parameters.

### Error responses

Assuming we're using the default implementation of __IHttpExceptionHandler__ and the above function was called with improper values in intParams, the function returns 400, with JSON content:

```
HTTP/1.1 400 Bad Request
Date: Sun, 13 Jan 2019 16:12:30 GMT
Content-Type: application/json
Server: Kestrel
Transfer-Encoding: chunked

{
  "message": "Failed to assign parameter 'intParams' value",
  "parameter": "intParams"
}
```

This functionality is provided by the exception filter.

Further examples can be found from the example project __AzureFunctionsV2.HttpExtensions.Examples.FunctionApp__ and documentation on exception
handling from the [wiki](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions/wiki).



## Documentation

The project comes with an example project in the sources [AzureFunctionsV2.HttpExtensions.Examples.FunctionApp](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions/tree/master/src/AzureFunctionsV2.HttpExtensions.Examples.FunctionApp) as well as with some [wiki](https://github.com/Jusas/AzureFunctionsV2.HttpExtensions/wiki) documentation. Please check the wiki
before opening an issue.


## How was this made?

By using Binding attributes it's possible to create Function parameters
that act as placeholders for the HttpRequest parameters, which are then assigned 
the proper values just before running the Function using Function Filters.

The caveat is that it's necessary to use a container (HttpParam<>) for the parameters
because the parameter binding happens before we can get access to the HttpRequest. The
values get bound kind of too early for this to work, but we save the day by having 
access to both the placeholder containers and the HttpRequest upon the Function Filter
running phase, which gets run just before the function gets called so we may assign
the values to each HttpParam's Value property there.

It's worth noticing that the Function Filters are a somewhat new feature and have been marked as obsolete - _however they're not obsolete, they have only been marked obsolete due to the team not having fully finished the features to consider them complete._ (see https://github.com/Azure/azure-webjobs-sdk/issues/1284)




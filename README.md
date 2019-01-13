# Azure Functions HTTP Extensions

## Note! Under construction, not ready for use!

This library adds useful extensions to HTTP triggered functions. It allows you to
add HTTP parameters from headers, query parameters, body and form fields directly
to the function signature. It also adds some boilerplate code to take advantage of
Function Filters in v2 Functions, allowing you to specify cross-cutting Exception 
handling tasks combined with an overridable error response formatter.

## How?

By using Binding attributes it's possible to create Function parameters
that act as placeholders for the HttpRequest parameters, which are then assigned 
the proper values just before running the Function using Function Filters.

The caveat is that it's necessary to use a container (HttpParam<>) for the parameters
because the parameter binding happens before we can get access to the HttpRequest. The
values get bound kind of too early for this to work, but we save the day by having 
access to both the placeholder containers and the HttpRequest upon the Function Filter
running phase, which gets run just before the function gets called so we may assign
the values to each HttpParam's Value property there.

## Some samples

### Function signature

```C#
[FunctionName("TestFunction")]
public static async Task<IActionResult> TestFunction (
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mymethod/{somestring}")] HttpRequest req,
    string somestring,
    [HttpQuery(Required = true)]HttpParam<string> stringParam,
    [HttpQuery]HttpParam<List<int>> intParams,
    [HttpQuery]HttpParam<TestEnum> enumParam,
    [HttpHeader(Name = "x-my-header")]HttpParam<string> customHeader,
    ILogger log)
{
	string blah = stringParam; // implicit operator

	foreach(var x in intParams.Value) {
		// ...
	}

    return new OkObjectResult("");
}
```

### Error response

Assuming we registered the default implementation of IHttpExceptionHandler and the above function was called 
with improper values in intParams, the function returns 400, with JSON content:

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



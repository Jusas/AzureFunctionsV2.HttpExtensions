using System;
using System.Collections.Generic;
using System.Text;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Moq;

namespace AzureFunctionsV2.HttpExtensions.Tests.Helpers
{
    public class MockedFunctionRequestContext
    {
        public Mock<ILogger> MockedLogger { get; set; }
        public Guid FunctionContextId { get; set; }
        public IHttpParam HttpParam { get; set; }
        public MockHttpRequest HttpRequest { get; set; }
        public FunctionExecutingContext FunctionExecutingContext { get; set; }
        public Dictionary<string, object> ArgumentsDictionary { get; set; } = new Dictionary<string, object>();
        public Mock<IHttpRequestStore> RequestStoreMock { get; set; }

        public MockedFunctionRequestContext()
        {
            MockedLogger = new Mock<ILogger>();
            FunctionContextId = Guid.NewGuid();
            HttpRequest = new MockHttpRequest();
            RequestStoreMock = new Mock<IHttpRequestStore>();
            RequestStoreMock.Setup(x => x.Get(FunctionContextId)).Returns(HttpRequest);
            ArgumentsDictionary.Add("httptrigger_request", HttpRequest);
            HttpRequest.ContentType = "application/json";
        }

        public HttpParam<T> AddFormHttpParam<T>(string argumentName, string alias = null)
        {
            var attribute = new HttpFormAttribute(alias);
            var arg = new HttpParam<T>() { HttpExtensionAttribute = attribute };
            ArgumentsDictionary.Add(argumentName, arg);
            FunctionExecutingContext = new FunctionExecutingContext(ArgumentsDictionary, 
                new Dictionary<string, object>(), FunctionContextId, "func", MockedLogger.Object);
            return arg;
        }

        public HttpParam<T> AddBodyHttpParam<T>(string argumentName)
        {
            var attribute = new HttpBodyAttribute();
            var arg = new HttpParam<T>() { HttpExtensionAttribute = attribute };
            ArgumentsDictionary.Add(argumentName, arg);
            FunctionExecutingContext = new FunctionExecutingContext(ArgumentsDictionary,
                new Dictionary<string, object>(), FunctionContextId, "func", MockedLogger.Object);
            return arg;
        }

        public HttpParam<T> AddHeaderHttpParam<T>(string argumentName, string alias = null)
        {
            var attribute = new HttpHeaderAttribute(alias ?? argumentName);
            var arg = new HttpParam<T>() { HttpExtensionAttribute = attribute };
            ArgumentsDictionary.Add(argumentName, arg);
            FunctionExecutingContext = new FunctionExecutingContext(ArgumentsDictionary,
                new Dictionary<string, object>(), FunctionContextId, "func", MockedLogger.Object);
            return arg;
        }

        public HttpParam<T> AddQueryHttpParam<T>(string argumentName, string alias = null)
        {
            var attribute = new HttpQueryAttribute(alias);
            var arg = new HttpParam<T>() { HttpExtensionAttribute = attribute };
            ArgumentsDictionary.Add(argumentName, arg);
            FunctionExecutingContext = new FunctionExecutingContext(ArgumentsDictionary,
                new Dictionary<string, object>(), FunctionContextId, "func", MockedLogger.Object);
            return arg;
        }
    }
}

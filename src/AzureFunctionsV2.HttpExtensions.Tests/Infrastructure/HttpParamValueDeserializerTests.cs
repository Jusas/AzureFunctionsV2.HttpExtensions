using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Infrastructure
{
    /// <summary>
    /// Tests for checking that any implementation for IHttpParamValueDeserializer is utilized
    /// correctly.
    /// </summary>
    public class HttpParamValueDeserializerTests
    {
        /// <summary>
        /// A custom deserializer that overrides deserialization functionality.
        /// </summary>
        public class CustomQueryParamDeserializer : IHttpParamValueDeserializer
        {
            public async Task<DeserializerResult> DeserializeBodyParameter(Stream body, Type httpParamValueType, string functionName, HttpRequest request)
            {
                using (var sr = new StreamReader(body))
                {
                    var data = await sr.ReadToEndAsync();
                    data = data + " from body deserializer";
                    return new DeserializerResult(true, data);
                }

            }

            public async Task<DeserializerResult> DeserializeHeaderParameter(string headerName, StringValues headerValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                var longVal = long.Parse(headerValue.ToString());
                return new DeserializerResult(true, longVal * 1000);
            }

            public async Task<DeserializerResult> DeserializeFormParameter(string formParameterName, StringValues formParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                string result = formParameterValue.ToString() + " from form deserializer";
                return new DeserializerResult(true, result);
            }

            public async Task<DeserializerResult> DeserializeQueryParameter(string queryParameterName, StringValues queryParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                if (queryParameterValue.ToString().EndsWith("foo"))
                    return new DeserializerResult(true, (int)1);

                return DeserializerResult.DidNotDeserialize;
            }

            public async Task<DeserializerResult> DeserializeFormFile(string fileParameterName, IFormFile formFile, Type httpParamValueType, string functionName, HttpRequest request)
            {
                using (var sr = new StreamReader(formFile.OpenReadStream()))
                {
                    var data = await sr.ReadToEndAsync();
                    data = data + " from file deserializer";
                    return new DeserializerResult(true, data);
                }
            }
        }

        /// <summary>
        /// A custom deserializer which in fact does nothing and just passes control back to the
        /// default deserialization code.
        /// </summary>
        public class CustomPassThroughQueryParamDeserializer : IHttpParamValueDeserializer
        {
            public async Task<DeserializerResult> DeserializeBodyParameter(Stream body, Type httpParamValueType, string functionName, HttpRequest request)
            => DeserializerResult.DidNotDeserialize;

            public async Task<DeserializerResult> DeserializeHeaderParameter(string headerName, StringValues headerValue, Type httpParamValueType,
                string functionName, HttpRequest request)
                => DeserializerResult.DidNotDeserialize;

            public async Task<DeserializerResult> DeserializeFormParameter(string formParameterName, StringValues formParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
                => DeserializerResult.DidNotDeserialize;

            public async Task<DeserializerResult> DeserializeQueryParameter(string queryParameterName, StringValues queryParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
                => DeserializerResult.DidNotDeserialize;

            public async Task<DeserializerResult> DeserializeFormFile(string fileParameterName, IFormFile formFile, Type httpParamValueType, string functionName,
                HttpRequest request)
                => DeserializerResult.DidNotDeserialize;
        }

        /// <summary>
        /// Tests that the custom deserializer is called for query parameters.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_deserialize_query_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var queryParam = mockedFunctionRequestContext.AddQueryHttpParam<int>("queryParam");
            mockedFunctionRequestContext.HttpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>() {{ "queryParam", "foo"}});

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();
            
            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal(1, queryParam.Value);
        }

        /// <summary>
        /// Tests that the custom deserializer is called for header parameters.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_deserialize_header_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            long headerValue = 1234;
            var headerParam = mockedFunctionRequestContext.AddHeaderHttpParam<long>("myHeader", "x-my-header");
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(new Dictionary<string, StringValues>()
            {
                {"x-my-header", $"{headerValue}"}
            });

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal(headerValue * 1000, headerParam.Value);
        }

        /// <summary>
        /// Tests that the custom deserializer is called for body parameters.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_deserialize_body_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            string bodyValue = "hello";
            var bodyBytes = Encoding.UTF8.GetBytes(bodyValue);
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<string>("body");
            mockedFunctionRequestContext.HttpRequest.Body = new MemoryStream(bodyBytes);

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("hello from body deserializer", bodyParam.Value);
        }

        /// <summary>
        /// Tests that the custom deserializer is called for form parameters.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_deserialize_form_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            string formFieldValue = "hello";
            var formParam = mockedFunctionRequestContext.AddFormHttpParam<string>("formField");
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>() {{"formField", new StringValues(formFieldValue)}});

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("hello from form deserializer", formParam.Value);
        }

        /// <summary>
        /// Tests that the custom deserializer is called for file parameters.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_deserialize_form_file_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();

            var mockedFileBytes = Encoding.UTF8.GetBytes("hello");
            MemoryStream mockedFileStream = new MemoryStream(mockedFileBytes);

            var formFileParam = mockedFunctionRequestContext.AddFormHttpParam<string>("file1");
            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection()
            {
                new FormFile(mockedFileStream, 0, mockedFileStream.Length, "file1", "test.txt")
            });

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();

            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal("hello from file deserializer", formFileParam.Value);
        }

        /// <summary>
        /// When returning false from custom deserialization methods, we should fall back to default
        /// deserialization code.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_run_custom_deserializer_but_deserialize_with_default_code_because_control_is_passed_back_by_not_deserializing()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.ContentType = "text/plain";

            var mockedFileBytes = Encoding.UTF8.GetBytes("fileValue");
            MemoryStream mockedFileStream = new MemoryStream(mockedFileBytes);

            var formFileParam = mockedFunctionRequestContext.AddFormHttpParam<Stream>("file1");
            var formParam = mockedFunctionRequestContext.AddFormHttpParam<string>("form");
            var headerParam = mockedFunctionRequestContext.AddHeaderHttpParam<string>("header");
            var queryParam = mockedFunctionRequestContext.AddQueryHttpParam<string>("query");
            var bodyParam = mockedFunctionRequestContext.AddBodyHttpParam<string>("body");
            var bodyBytes = Encoding.UTF8.GetBytes("bodyValue");
            var bodyStream = new MemoryStream(bodyBytes);

            mockedFunctionRequestContext.HttpRequest.Form = new FormCollection(
                new Dictionary<string, StringValues>()
                {
                    {"form", "formValue"}
                }, 
                new FormFileCollection()
                {
                    new FormFile(mockedFileStream, 0, mockedFileStream.Length, "file1", "test.txt")
                });
            mockedFunctionRequestContext.HttpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                {"query", "queryValue"}
            });
            mockedFunctionRequestContext.HttpRequest.Body = bodyStream;
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(new Dictionary<string, StringValues>()
            {
                {"header", "headerValue"}
            });

            var passThroughQueryParamDeserializer = new CustomPassThroughQueryParamDeserializer();
            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, 
                passThroughQueryParamDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            formFileParam.Value.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(formFileParam.Value))
            {
                var fileParamContentString = sr.ReadToEnd();
                Assert.Equal("fileValue", fileParamContentString);
            }

            Assert.Equal("formValue", formParam.Value);
            Assert.Equal("queryValue", queryParam.Value);
            Assert.Equal("headerValue", headerParam.Value);
            Assert.Equal("bodyValue", bodyParam.Value);
        }

        /// <summary>
        /// Verify that OnExecutedAsync will clean up and remove the HttpRequest from request store when
        /// the invocation is completed.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Should_remove_http_request_from_request_store_after_function_invocation()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var executedContext = new FunctionExecutedContext(new Dictionary<string, object>(), new Dictionary<string, object>(),
                Guid.Empty, "func", new Mock<ILogger>().Object, new FunctionResult(true));
            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await httpParamAssignmentFilter.OnExecutedAsync(executedContext, new CancellationToken());

            // Assert
            mockedFunctionRequestContext.RequestStoreMock.Verify(x => x.Remove(Guid.Empty), Times.Once);
        }
    }
}

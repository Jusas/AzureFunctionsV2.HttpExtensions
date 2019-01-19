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
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests
{
    public class HttpParamValueDeserializerTests
    {

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


    }
}

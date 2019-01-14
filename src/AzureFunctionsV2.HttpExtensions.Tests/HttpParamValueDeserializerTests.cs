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
                throw new NotImplementedException();
            }

            public async Task<DeserializerResult> DeserializeHeaderParameter(string headerName, StringValues headerValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                throw new NotImplementedException();
            }

            public async Task<DeserializerResult> DeserializeFormParameter(string formParameterName, StringValues formParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                throw new NotImplementedException();
            }

            public async Task<DeserializerResult> DeserializeQueryParameter(string queryParameterName, StringValues queryParameterValue, Type httpParamValueType,
                string functionName, HttpRequest request)
            {
                if (queryParameterValue.ToString().EndsWith("foo"))
                    return new DeserializerResult(true, (int)1);

                return DeserializerResult.DidNotDeserialize;
            }
        }

        [Fact]
        public async Task Should_serialize_query_parameter_with_custom_deserializer()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var queryParam = mockedFunctionRequestContext.AddQueryHttpParam<int>("queryparam");
            mockedFunctionRequestContext.HttpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>() {{"queryparam", "foo"}});

            var simpleParamValueDeserializer = new CustomQueryParamDeserializer();
            
            var httpParamAssignmentFilter = new HttpParamAssignmentFilter(mockedFunctionRequestContext.RequestStoreMock.Object, simpleParamValueDeserializer);

            // Act
            await httpParamAssignmentFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext,
                new CancellationToken());

            // Assert
            Assert.Equal(1, queryParam.Value);
        }
    }
}

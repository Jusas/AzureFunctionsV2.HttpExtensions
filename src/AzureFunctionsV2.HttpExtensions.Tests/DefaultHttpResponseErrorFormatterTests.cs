using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests
{
    public class DefaultHttpResponseErrorFormatterTests
    {
        [Fact]
        public async Task Should_write_400_on_HttpExtensionsExceptions_and_write_a_JSON_object_to_body()
        {
            // Arrange
            var errorFormatter = new DefaultHttpResponseErrorFormatter();
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.GenerateExceptionContext(new ParameterFormatConversionException("Test exception", 
                null, "testparameter", mockedFunctionRequestContext.HttpContext));

            // Act
            await errorFormatter.WriteErrorResponse(mockedFunctionRequestContext.FunctionExceptionContext,
                mockedFunctionRequestContext.HttpResponse);

            // Assert
            var bodyString = "";
            mockedFunctionRequestContext.HttpResponse.Body.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(mockedFunctionRequestContext.HttpResponse.Body))
            {
                bodyString = sr.ReadToEnd();
            }
            Assert.Equal(400, mockedFunctionRequestContext.HttpResponse.StatusCode);
            Assert.StartsWith("{", bodyString);
            Assert.EndsWith("}", bodyString);
        }

        [Fact]
        public async Task Should_write_500_on_general_exceptions_and_write_a_JSON_object_to_body()
        {
            // Arrange
            var errorFormatter = new DefaultHttpResponseErrorFormatter();
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.GenerateExceptionContext(new ArgumentOutOfRangeException());

            // Act
            await errorFormatter.WriteErrorResponse(mockedFunctionRequestContext.FunctionExceptionContext,
                mockedFunctionRequestContext.HttpResponse);

            // Assert
            var bodyString = "";
            mockedFunctionRequestContext.HttpResponse.Body.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(mockedFunctionRequestContext.HttpResponse.Body))
            {
                bodyString = sr.ReadToEnd();
            }
            Assert.Equal(500, mockedFunctionRequestContext.HttpResponse.StatusCode);
            Assert.StartsWith("{", bodyString);
            Assert.EndsWith("}", bodyString);
        }
    }
}

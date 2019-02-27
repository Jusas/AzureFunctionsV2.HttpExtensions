using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Infrastructure
{
    [CollectionDefinition("Non-Parallel DefaultHttpExceptionHandlerTests", DisableParallelization = true)]
    public class DefaultHttpExceptionHandlerTestsCollectionDefinition
    {
    }

    [Collection("Non-Parallel DefaultHttpExceptionHandlerTests")]
    public class DefaultHttpExceptionHandlerTests
    {
        [Fact]
        public async Task Should_output_BadRequest_400_on_HttpExtensionsException()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var exception = new ParameterFormatConversionException("Test exception", null, "x", mockedFunctionRequestContext.HttpContext);
            var handler = new DefaultHttpExceptionHandler();
            DefaultHttpExceptionHandler.OutputRecursiveExceptionMessages = false;

            // Act
            var result = await handler.HandleException(mockedFunctionRequestContext.FunctionExecutingContext,
                mockedFunctionRequestContext.HttpContext.Request, exception);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task Should_output_InternalError_500_on_general_exceptions_and_output_recursive_exception_message()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var exception = new Exception("outer", new Exception("inner"));
            var handler = new DefaultHttpExceptionHandler();
            DefaultHttpExceptionHandler.OutputRecursiveExceptionMessages = true;

            // Act
            var result = await handler.HandleException(mockedFunctionRequestContext.FunctionExecutingContext,
                mockedFunctionRequestContext.HttpContext.Request, exception);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(500, objectResult.StatusCode);
            JsonConvert.SerializeObject(objectResult.Value).Should().Contain("inner").And.Contain("outer");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Infrastructure
{
    public class HttpExtensionsExceptionFilterTests
    {
        //[Fact]
        //public async Task Should_call_http_exception_handler_upon_exception()
        //{
        //    // Arrange
        //    var mockedFunctionRequestContext = new MockedFunctionRequestContext();
        //    mockedFunctionRequestContext.RequestStoreMock.Setup(x => x.Get(Guid.Empty))
        //        .Returns(mockedFunctionRequestContext.HttpRequest);
        //    var exceptionHandlerMock = new Mock<IHttpExceptionHandler>();
        //    var exceptionFilter = new HttpExtensionsExceptionFilter(
        //        mockedFunctionRequestContext.RequestStoreMock.Object,
        //        exceptionHandlerMock.Object);
        //    var exception = new Exception("test");
        //    var exceptionContext = new FunctionExceptionContext(Guid.Empty, "func",
        //        mockedFunctionRequestContext.MockedLogger.Object, ExceptionDispatchInfo.Capture(exception),
        //        new Dictionary<string, object>());
            
        //    // Act
        //    await exceptionFilter.OnExceptionAsync(exceptionContext, new CancellationToken());

        //    // Assert
        //    exceptionHandlerMock.Verify(x => x.HandleException(
        //        It.Is<FunctionExceptionContext>((value) => value == exceptionContext), 
        //        It.Is<HttpContext>((value) => value == mockedFunctionRequestContext.HttpContext)), Times.Once);

        //}
    }
}

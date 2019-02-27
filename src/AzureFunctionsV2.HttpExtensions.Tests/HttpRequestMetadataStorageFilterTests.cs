using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using AzureFunctionsV2.HttpExtensions.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests
{
    public class HttpRequestMetadataStorageFilterTests
    {
        [Fact]
        public async Task Should_add_HttpRequest_and_FunctionExecutingContext_to_request_store_after_function_invocation()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            // TODO use above in parameter and check the guid
            var metadataStorageFilter = new HttpRequestMetadataStorageFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await metadataStorageFilter.OnExecutingAsync(executingContext, new CancellationToken());

            // Assert
            mockedFunctionRequestContext.RequestStoreMock.Verify(x => x.Set(Guid.Empty, mockedFunctionRequestContext.HttpRequest), Times.Once);
            mockedFunctionRequestContext.HttpContext.GetStoredFunctionExecutingContext().Should().NotBeNull();
        }

        [Fact]
        public async Task Should_remove_HttpRequest_from_request_store_after_function_invocation()
        {
            // Arrange
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            var executedContext = new FunctionExecutedContext(new Dictionary<string, object>(), new Dictionary<string, object>(),
                Guid.Empty, "func", new Mock<ILogger>().Object, new FunctionResult(true));
            var metadataStorageFilter = new HttpRequestMetadataStorageFilter(mockedFunctionRequestContext.RequestStoreMock.Object, null);

            // Act
            await metadataStorageFilter.OnExecutedAsync(executedContext, new CancellationToken());

            // Assert
            mockedFunctionRequestContext.RequestStoreMock.Verify(x => x.Remove(Guid.Empty), Times.Once);
        }
    }
}

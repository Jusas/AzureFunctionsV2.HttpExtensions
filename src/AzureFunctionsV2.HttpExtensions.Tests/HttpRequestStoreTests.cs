using System;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests
{
    public class HttpRequestStoreTests
    {
        [Fact]
        public void Should_store_and_return_and_delete_request()
        {
            // Arrange
            var requestStore = new HttpRequestStore();
            Guid guid = Guid.NewGuid();

            // Act
            var newRequest = new MockHttpRequest(null);
            requestStore.Set(guid, newRequest);
            var returnedRequest = requestStore.Get(guid);
            requestStore.Remove(guid);
            var shouldBeNull = requestStore.Get(guid);

            // Assert
            Assert.Equal(newRequest, returnedRequest);
            Assert.Null(shouldBeNull);

        }
    }
}

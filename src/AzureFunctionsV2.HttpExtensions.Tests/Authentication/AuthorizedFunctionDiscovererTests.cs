using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using FluentAssertions;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Authentication
{
    public class AuthorizedFunctionDiscovererTests
    {
        [Fact]
        public async Task Should_find_functions_with_HttpAuthorizeAttribute()
        {
            // Arrange
            var discoverer = new AuthorizedFunctionDiscoverer();

            // Act
            var functions = discoverer.GetFunctions();

            // Assert
            functions.Keys.Should().Contain(new[] {"AuthTest1", "AuthTest2", "AuthTest4", "AuthTest5", "AuthTest6"});
        }

        [Fact]
        public async Task Should_not_include_functions_without_HttpAuthorizeAttribute()
        {
            // Arrange
            var discoverer = new AuthorizedFunctionDiscoverer();

            // Act
            var functions = discoverer.GetFunctions();

            // Assert
            functions.Keys.Should().NotContain(new[] { "AuthTest3", "HeaderTestFunction-Primitives", "QueryParameterTestFunction-Basic" });
        }

    }
}
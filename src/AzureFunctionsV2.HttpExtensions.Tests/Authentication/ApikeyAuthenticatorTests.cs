using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Authentication
{
    public class ApiKeyAuthenticatorTests
    {
        [Fact]
        public async Task Should_throw_without_valid_configuration()
        {
            // Arrange

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (new ApiKeyAuthenticator(null).Authenticate("foo", null)));
        }

        [Fact]
        public async Task Should_authenticate_successfully()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo"
                }
            });

            // Act
            var basicAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate("foo", new MockHttpRequest(new MockHttpContext()));
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Should_fail_authentication()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo"
                }
            });

            // Act
            var basicAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate("bar", new MockHttpRequest(new MockHttpContext()));

            // Assert
            Assert.False(result);
        }
    }
}
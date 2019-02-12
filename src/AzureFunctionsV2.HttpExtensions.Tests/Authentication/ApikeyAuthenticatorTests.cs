using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (new ApiKeyAuthenticator(null).Authenticate(null)));
        }

        [Fact]
        public async Task Should_authenticate_successfully_using_header()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo",
                    HeaderName = "x-key"
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            httpRequest.HeaderDictionary = new HeaderDictionary() {{"x-key", "foo"}};

            // Act
            var apiKeyAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await apiKeyAuthenticator.Authenticate(httpRequest);
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Should_authenticate_successfully_using_query_param()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo",
                    QueryParameterName = "key"
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            httpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>() {{"key", "foo"}});

            // Act
            var apiKeyAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await apiKeyAuthenticator.Authenticate(httpRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Should_authenticate_successfully_using_query_param_when_both_header_and_queryparam_are_configured()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo",
                    QueryParameterName = "key",
                    HeaderName = "x-key"
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            httpRequest.HeaderDictionary = new HeaderDictionary();
            httpRequest.Query = new QueryCollection(new Dictionary<string, StringValues>() { { "key", "foo" } });

            // Act
            var apiKeyAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await apiKeyAuthenticator.Authenticate(httpRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Should_fail_authentication_with_wrong_key_using_header()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo",
                    HeaderName = "x-key"
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            httpRequest.HeaderDictionary = new HeaderDictionary() { { "x-key", "x" } };

            // Act
            var basicAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate(httpRequest);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Should_fail_authentication_when_not_providing_credentials()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (key, request) => key == "foo",
                    HeaderName = "x-key"
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            httpRequest.Query = new QueryCollection();
            httpRequest.HeaderDictionary = new HeaderDictionary();

            // Act
            var basicAuthenticator = new ApiKeyAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate(httpRequest);

            // Assert
            Assert.False(result);
        }
    }
}
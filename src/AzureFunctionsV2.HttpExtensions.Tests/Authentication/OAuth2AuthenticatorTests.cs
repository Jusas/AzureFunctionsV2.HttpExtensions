using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Tests.Mocks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Authentication
{
    public class OAuth2AuthenticatorTests
    {
        [Fact]
        public async Task Should_throw_without_valid_configuration()
        {
            // Arrange

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (new OAuth2Authenticator(null).AuthenticateAndAuthorize("", null, null)));
        }

        [Fact]
        public async Task Should_throw_with_non_bearer_tokens()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                OAuth2Authentication = new OAuth2AuthenticationParameters()
                {
                    CustomAuthorizationFilter = async (token, request, attributes) => new ClaimsPrincipal()
                }
            });
            // Act

            // Assert
            await Assert.ThrowsAsync<HttpAuthenticationException>(async () => await (new OAuth2Authenticator(config.Object).AuthenticateAndAuthorize("foo", null, null)));
        }

        [Fact]
        public async Task Should_authenticate_successfully()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                OAuth2Authentication = new OAuth2AuthenticationParameters()
                {
                    CustomAuthorizationFilter = async (token, request, attributes) => new ClaimsPrincipal()
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            var authAttributes = new List<HttpAuthorizeAttribute>() {new HttpAuthorizeAttribute(Scheme.OAuth2)};

            // Act
            var oauth2Authenticator = new OAuth2Authenticator(config.Object);
            var result = await oauth2Authenticator.AuthenticateAndAuthorize("Bearer foo", httpRequest, authAttributes);
            
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Should_fail_authentication()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                OAuth2Authentication = new OAuth2AuthenticationParameters()
                {
                    CustomAuthorizationFilter = async (token, request, attributes) => throw new HttpAuthorizationException("unauthorized")
                }
            });
            var httpRequest = new MockHttpRequest(new MockHttpContext());
            var authAttributes = new List<HttpAuthorizeAttribute>() { new HttpAuthorizeAttribute(Scheme.OAuth2) };

            // Act
            var oauth2Authenticator = new OAuth2Authenticator(config.Object);
            await Assert.ThrowsAsync<HttpAuthorizationException>(async () => await oauth2Authenticator.AuthenticateAndAuthorize("Bearer foo", httpRequest, authAttributes));

        }
    }
}
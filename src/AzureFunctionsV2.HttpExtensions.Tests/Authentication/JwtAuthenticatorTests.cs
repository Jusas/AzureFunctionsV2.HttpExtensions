﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Authentication
{
    public class JwtAuthenticatorTests
    {
        [Fact]
        public async Task Should_throw_without_valid_configuration()
        {
            // Arrange

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (new JwtAuthenticator(null, null, null).Authenticate("foo")));
        }

        [Fact]
        public async Task
            Should_get_and_use_config_from_OIDC_endpoint_when_OpenIdConnectJwtValidationParameters_is_used()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            var optionsObject = new HttpAuthenticationOptions()
            {
                JwtAuthentication = new JwtAuthenticationParameters()
                {
                    TokenValidationParameters = new OpenIdConnectJwtValidationParameters()
                    {
                        OpenIdConnectConfigurationUrl = "http://foo.bar"
                    }
                }
            };
            config.SetupGet(opts => opts.Value).Returns(optionsObject);
            var tokenValidator = new Mock<ISecurityTokenValidator>();
            var configManager = new Mock<IConfigurationManager<OpenIdConnectConfiguration>>();
            configManager.Setup(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OpenIdConnectConfiguration());

            // Act
            var jwtAuthenticator = new JwtAuthenticator(config.Object, tokenValidator.Object, configManager.Object);
            await jwtAuthenticator.Authenticate("Bearer foo");
            
            // Assert
            SecurityToken validatedToken;
            tokenValidator.Verify(
                x => x.ValidateToken("foo", optionsObject.JwtAuthentication.TokenValidationParameters, out validatedToken),
                Times.Once);
            configManager.Verify(x => x.GetConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Text;
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
    public class BasicAuthenticatorTests
    {
        [Fact]
        public async Task Should_throw_without_valid_configuration()
        {
            // Arrange

            // Act

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await (new BasicAuthenticator(null).Authenticate("foo")));
        }

        [Fact]
        public async Task Should_authenticate_successfully()
        {
            // Arrange
            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                BasicAuthentication = new BasicAuthenticationParameters()
                {
                    ValidCredentials = new Dictionary<string, string>() {
                        {"user", "pass"}
                    }
                }
            });

            // Act
            var basicAuthenticator = new BasicAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:pass")));
            
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
                BasicAuthentication = new BasicAuthenticationParameters()
                {
                    ValidCredentials = new Dictionary<string, string>() {
                        {"user", "pass"}
                    }
                }
            });

            // Act
            var basicAuthenticator = new BasicAuthenticator(config.Object);
            var result = await basicAuthenticator.Authenticate("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("foo:bar")));

            // Assert
            Assert.False(result);
        }
    }
}
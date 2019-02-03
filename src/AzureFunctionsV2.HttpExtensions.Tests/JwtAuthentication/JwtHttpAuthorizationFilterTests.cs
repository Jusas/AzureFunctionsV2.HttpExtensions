using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.JwtAuthentication
{
    public class JwtHttpAuthorizationFilterTests
    {

        [Fact]
        public async Task Should_run_auth_code_for_Function_which_has_HttpJwtAuthorizeAttribute_and_succeed()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(), new JwtSecurityToken());
            });
            var discoverer = new Mock<IJwtAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>()
                {
                    // No claims.
                    {"func", (method.Object, new HttpJwtAuthorizeAttribute())}
                });

            var authFilter = new JwtHttpAuthorizationFilter(jwtAuthenticator.Object, discoverer.Object, null);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(
                new Dictionary<string, StringValues>() {{"Authorization", new StringValues("Bearer foo") }});
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act
            await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None);

            // Assert
            Assert.NotNull(userParam.ClaimsPrincipal);
        }

        [Fact]
        public async Task Should_run_auth_code_for_Function_which_has_HttpJwtAuthorizeAttribute_and_claims_and_succeed()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim("myClaim", "myValue") })), new JwtSecurityToken());
            });
            var discoverer = new Mock<IJwtAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>()
                {
                    {"func", (method.Object, new HttpJwtAuthorizeAttribute()
                    {
                        ClaimType = "myClaim",
                        ClaimValue = "myValue"
                    })}
                });

            var authFilter = new JwtHttpAuthorizationFilter(jwtAuthenticator.Object, discoverer.Object, null);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(
                new Dictionary<string, StringValues>() { { "Authorization", new StringValues("Bearer foo") } });
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act
            await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None);

            // Assert
            Assert.NotNull(userParam.ClaimsPrincipal);
        }

        [Fact]
        public async Task Should_run_custom_authorization_filter()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim("myClaim", "myValue") })), new JwtSecurityToken());
            });
            var discoverer = new Mock<IJwtAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>()
                {
                    {"func", (method.Object, new HttpJwtAuthorizeAttribute()
                    {
                        ClaimType = "myClaim",
                        ClaimValue = "myValue"
                    })}
                });


            var options = new Mock<IOptions<JwtAuthenticationOptions>>();
            options.SetupGet(x => x.Value).Returns(new JwtAuthenticationOptions()
            {
                CustomAuthorizationFilter = async (principal, token) => throw new HttpAuthorizationException("custom")
            });
            var authFilter = new JwtHttpAuthorizationFilter(jwtAuthenticator.Object, discoverer.Object, options.Object);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(
                new Dictionary<string, StringValues>() { { "Authorization", new StringValues("Bearer foo") } });
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpAuthorizationException>(async () =>
                await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None));
            Assert.Equal("custom", exception.Message);
        }

        [Fact]
        public async Task Should_run_auth_code_for_Function_which_has_HttpJwtAuthorizeAttribute_and_claims_and_fail_with_missing_claims()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(), new JwtSecurityToken());
            });
            var discoverer = new Mock<IJwtAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>()
                {
                    {"func", (method.Object, new HttpJwtAuthorizeAttribute()
                    {
                        ClaimType = "myClaim",
                        ClaimValue = "myValue"
                    })}
                });

            var authFilter = new JwtHttpAuthorizationFilter(jwtAuthenticator.Object, discoverer.Object, null);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(
                new Dictionary<string, StringValues>() { { "Authorization", new StringValues("Bearer foo") } });
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act & Assert
            await Assert.ThrowsAsync<HttpAuthorizationException>(async () => 
                await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None));

        }

        [Fact]
        public async Task Should_not_run_auth_code_for_Function_which_does_not_have_HttpJwtAuthorizeAttribute()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(), new JwtSecurityToken());
            });
            var discoverer = new Mock<IJwtAuthorizedFunctionDiscoverer>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>());

            var authFilter = new JwtHttpAuthorizationFilter(jwtAuthenticator.Object, discoverer.Object, null);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary();
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act
            await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None);

            // Assert
            Assert.Null(userParam.ClaimsPrincipal);
        }
    }
}

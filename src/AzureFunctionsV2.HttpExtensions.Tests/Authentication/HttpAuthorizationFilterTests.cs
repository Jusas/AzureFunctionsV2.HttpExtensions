using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace AzureFunctionsV2.HttpExtensions.Tests.Authentication
{
    public class HttpAuthorizationFilterTests
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
            var discoverer = new Mock<IAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, IList<HttpJwtAuthorizeAttribute>)>()
                {
                    {"func", (method.Object, new List<HttpJwtAuthorizeAttribute>() {new HttpJwtAuthorizeAttribute()})}
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
        public async Task Should_run_custom_authorization_filter()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>() { new Claim("myClaim", "myValue") })), new JwtSecurityToken());
            });
            var discoverer = new Mock<IAuthorizedFunctionDiscoverer>();
            var method = new Mock<MethodInfo>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, IList<HttpJwtAuthorizeAttribute>)>()
                {
                    {"func", (method.Object, new List<HttpJwtAuthorizeAttribute>() {new HttpJwtAuthorizeAttribute()})}
                });


            var options = new Mock<IOptions<JwtAuthenticationOptions>>();
            options.SetupGet(x => x.Value).Returns(new JwtAuthenticationOptions()
            {
                CustomAuthorizationFilter = async (principal, token, authorizeAttrs) => throw new HttpAuthorizationException("custom")
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
        public async Task Should_not_run_auth_code_for_Function_which_does_not_have_HttpJwtAuthorizeAttribute()
        {
            // Arrange
            var jwtAuthenticator = new Mock<IJwtAuthenticator>();
            jwtAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>())).ReturnsAsync(() =>
            {
                return (new ClaimsPrincipal(), new JwtSecurityToken());
            });
            var discoverer = new Mock<IAuthorizedFunctionDiscoverer>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, IList<HttpJwtAuthorizeAttribute>)>());

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

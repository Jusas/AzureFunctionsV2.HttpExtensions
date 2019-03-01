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
        public async Task Should_run_JwtAuthenticator_code_for_Function_which_has_HttpAuthorizeAttribute_Jwt_and_succeed()
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
                new Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)>()
                {
                    {"func", (method.Object, new List<HttpAuthorizeAttribute>() {new HttpAuthorizeAttribute(Scheme.Jwt)})}
                });

            var config = new Mock<IOptions<HttpAuthenticationOptions>>();
            config.SetupGet(opts => opts.Value).Returns(new HttpAuthenticationOptions()
            {
                JwtAuthentication = new JwtAuthenticationParameters()
                {
                    TokenValidationParameters = new OpenIdConnectJwtValidationParameters()
                    {
                        OpenIdConnectConfigurationUrl = "http://foo.bar"
                    }
                }
            });

            var authFilter = new HttpAuthorizationFilter(discoverer.Object, config.Object, jwtAuthenticator.Object, null, null, null);
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
        public async Task Should_run_custom_authorization_method_with_Jwt_auth()
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
                new Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)>()
                {
                    {"func", (method.Object, new List<HttpAuthorizeAttribute>() {new HttpAuthorizeAttribute(Scheme.Jwt)})}
                });


            var options = new Mock<IOptions<HttpAuthenticationOptions>>();
            bool wasRun = false;
            options.SetupGet(x => x.Value).Returns(new HttpAuthenticationOptions()
            {
                JwtAuthentication = new JwtAuthenticationParameters()
                {
                    CustomAuthorizationFilter = async (principal, token, attributes) => wasRun = true
                }
            });
            var authFilter = new HttpAuthorizationFilter(discoverer.Object, options.Object, jwtAuthenticator.Object, null, null, null);
            var mockedFunctionRequestContext = new MockedFunctionRequestContext();
            mockedFunctionRequestContext.HttpRequest.HeaderDictionary = new HeaderDictionary(
                new Dictionary<string, StringValues>() { { "Authorization", new StringValues("Bearer foo") } });
            var userParam = mockedFunctionRequestContext.AddUserParam("user");
            mockedFunctionRequestContext.CreateFunctionExecutingContextWithJustName("func");

            // Act
            await authFilter.OnExecutingAsync(mockedFunctionRequestContext.FunctionExecutingContext, CancellationToken.None);
            Assert.True(wasRun);
        }

        [Fact]
        public async Task Should_not_run_any_auth_code_for_Function_which_does_not_have_HttpAuthorizeAttribute()
        {
            // Arrange
            var discoverer = new Mock<IAuthorizedFunctionDiscoverer>();
            discoverer.Setup(x => x.GetFunctions()).Returns(
                new Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)>());

            var authFilter = new HttpAuthorizationFilter(discoverer.Object, null, null, null, null, null);
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

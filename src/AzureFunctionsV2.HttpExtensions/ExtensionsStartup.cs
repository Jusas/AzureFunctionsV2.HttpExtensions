using System.IdentityModel.Tokens.Jwt;
using AzureFunctionsV2.HttpExtensions;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.IL;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

[assembly: WebJobsStartup(typeof(ExtensionsStartup), "AzureFunctionsV2HttpExtensionsStartup")]

namespace AzureFunctionsV2.HttpExtensions
{
    public class ExtensionsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddAzureFunctionHttpExtensions();

            builder.Services.AddSingleton<IHttpRequestStore, HttpRequestStore>();
            builder.Services.AddSingleton<IFunctionFilter, HttpRequestMetadataStorageFilter>();
            builder.Services.AddSingleton<IFunctionFilter, HttpParamAssignmentFilter>();
            
            builder.Services.AddSingleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>();
            DefaultHttpExceptionHandler.OutputRecursiveExceptionMessages = true;

            builder.Services.AddSingleton<IILFunctionExceptionHandler, ILFunctionExceptionHandler>();
            
            builder.Services.AddSingleton<IAuthorizedFunctionDiscoverer, AuthorizedFunctionDiscoverer>();
            builder.Services.AddSingleton<IFunctionFilter, HttpAuthorizationFilter>();

            builder.Services.AddSingleton<IJwtAuthenticator, JwtAuthenticator>(provider =>
            {
                var options = provider.GetService<IOptions<HttpAuthenticationOptions>>();
                var configManager =
                    options?.Value?.JwtAuthentication?.TokenValidationParameters is OpenIdConnectJwtValidationParameters oidcParams
                        ? new ConfigurationManager<OpenIdConnectConfiguration>(
                            oidcParams.OpenIdConnectConfigurationUrl, new OpenIdConnectConfigurationRetriever())
                        : null;
                return new JwtAuthenticator(options, new JwtSecurityTokenHandler(), configManager);
            });
            builder.Services.AddSingleton<IBasicAuthenticator, BasicAuthenticator>();
            builder.Services.AddSingleton<IOAuth2Authenticator, OAuth2Authenticator>();
            builder.Services.AddSingleton<IApiKeyAuthenticator, ApiKeyAuthenticator>();
        }
    }
}
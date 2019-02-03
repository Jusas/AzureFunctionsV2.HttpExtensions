using System.IdentityModel.Tokens.Jwt;
using AzureFunctionsV2.HttpExtensions;
using AzureFunctionsV2.HttpExtensions.Authorization;
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
            builder.Services.AddSingleton<IFunctionFilter, HttpParamAssignmentFilter>();
            builder.Services.AddSingleton<IFunctionFilter, HttpExtensionsExceptionFilter>();
            builder.Services.AddSingleton<IHttpRequestStore, HttpRequestStore>();
            builder.Services.AddSingleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>();
            builder.Services.AddSingleton<IHttpResponseErrorFormatter, DefaultHttpResponseErrorFormatter>();

            builder.Services.AddSingleton<IJwtAuthorizedFunctionDiscoverer, JwtAuthorizedFunctionDiscoverer>();
            builder.Services.AddSingleton<IFunctionFilter, JwtHttpAuthorizationFilter>();
            builder.Services.AddSingleton<IJwtAuthenticator, JwtAuthenticator>(provider =>
            {
                var options = provider.GetService<IOptions<JwtAuthenticationOptions>>();
                var configManager =
                    options?.Value.TokenValidationParameters is OpenIdConnectJwtValidationParameters oidcParams
                        ? new ConfigurationManager<OpenIdConnectConfiguration>(
                            oidcParams.OpenIdConnectConfigurationUrl, new OpenIdConnectConfigurationRetriever())
                        : null;
                return new JwtAuthenticator(options, new JwtSecurityTokenHandler(), configManager);
            });

        }
    }
}
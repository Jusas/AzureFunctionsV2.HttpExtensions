using System.Collections.Generic;
using System.Security.Claims;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup), "MyStartup")]

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup
{
    // See: https://github.com/Azure/Azure-Functions/issues/972
    // This is why the startup class is now in a separate assembly.
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // To replace default implementations:
            // builder.Services.Replace(ServiceDescriptor.Singleton<IHttpExceptionHandler, MyHttpExceptionHandler>());
            // builder.Services.Replace(ServiceDescriptor.Singleton<IHttpResponseErrorFormatter, MyHttpResponseErrorFormatter>());

            // Registering OIDC token validation parameters for the JWT authentication.
            builder.Services.Configure<JwtAuthenticationOptions>(options =>
            {
                options.TokenValidationParameters = new OpenIdConnectJwtValidationParameters()
                {
                    OpenIdConnectConfigurationUrl = "https://jusas-tests.eu.auth0.com/.well-known/openid-configuration",
                    ValidAudiences = new List<string>() { "http://localhost:7071/", "XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB" },
                    ValidateIssuerSigningKey = true,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
                // options.CustomAuthorizationFilter = async (principal, token) => { };
            });

            // Alternatively, if there is no OIDC endpoint and I want to define the public certificate and do things manually:
            /*
            builder.Services.Configure<JwtAuthenticationOptions>(options =>
            {
                string publicCert = @"my-base64-encoded-certificate";
                var x509cert = new X509Certificate2(Convert.FromBase64String(publicCert));
                SecurityKey sk = new X509SecurityKey(x509cert);
                sk.KeyId = x509cert.Thumbprint;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuers = new List<string>() { "https://my-issuer" },
                    ValidAudiences = new List<string>() { "my-audience" },
                    IssuerSigningKeys = new List<SecurityKey>() { sk },
                    ValidateIssuerSigningKey = true,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            */
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Examples.Authorization.Startup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(MyStartup), "MyStartup")]

namespace AzureFunctionsV2.HttpExtensions.Examples.Authorization.Startup
{
    public class MyStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.Configure<HttpAuthenticationOptions>(options =>
            {
                options.ApiKeyAuthentication = new ApiKeyAuthenticationParameters()
                {
                    ApiKeyVerifier = async (s, request) => s == "key" ? true : false,
                    HeaderName = "x-apikey"
                };
                options.BasicAuthentication = new BasicAuthenticationParameters()
                {
                    ValidCredentials = new Dictionary<string, string>() { { "admin", "admin" } }
                };
                options.JwtAuthentication = new JwtAuthenticationParameters()
                {
                    TokenValidationParameters = new OpenIdConnectJwtValidationParameters()
                    {
                        OpenIdConnectConfigurationUrl =
                            "https://jusas-tests.eu.auth0.com/.well-known/openid-configuration",
                        ValidAudiences = new List<string>()
                            {"XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB"},
                        ValidateIssuerSigningKey = true,
                        NameClaimType = ClaimTypes.NameIdentifier
                    },
                    CustomAuthorizationFilter = async (principal, token, attributes) => { }
                };
            });
        }
    }
}

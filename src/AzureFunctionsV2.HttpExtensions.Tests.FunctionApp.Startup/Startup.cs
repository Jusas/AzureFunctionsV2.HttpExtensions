using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

[assembly: WebJobsStartup(typeof(Startup), "MyStartup")]

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup
{
    // See: https://github.com/Azure/Azure-Functions/issues/972
    // This is why the startup class is now in a separate assembly.
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // builder.Services.Replace(ServiceDescriptor.Singleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>());
            // builder.Services.AddSingleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>();
            // builder.Services.AddSingleton<IHttpResponseErrorFormatter, DefaultHttpResponseErrorFormatter>();

            //builder.Services.AddSingleton<IJwtAuthenticator, JwtAuthenticator>(provider =>
            //{
            //    var oidcConfig = new OpenIdConnectJwtValidationParameters()
            //    {
            //        OpenIdConnectConfigurationUrl = "https://jusas-tests.eu.auth0.com/.well-known/openid-configuration",
            //        ValidAudiences = new List<string>() { "http://localhost:7071/", "XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB" },
            //        ValidateIssuerSigningKey = true,
            //        NameClaimType = ClaimTypes.NameIdentifier
            //    };
            //    return new JwtAuthenticator(oidcConfig);
            //});

            builder.Services.AddSingleton<IJwtAuthenticator, JwtAuthenticator>(provider =>
            {

                string publicCert =
                    @"MIIDCzCCAfOgAwIBAgIJGdy7cem4jJO5MA0GCSqGSIb3DQEBCwUAMCMxITAfBgNVBAMTGGp1c2FzLXRlc3RzLmV1LmF1dGgwLmNvbTAeFw0xOTAyMDIxNDI4MDBaFw0zMjEwMTExNDI4MDBaMCMxITAfBgNVBAMTGGp1c2FzLXRlc3RzLmV1LmF1dGgwLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAL95QDlemsbOwI5XBOZN6yh4GU+R+B6JxC4IG+DJPWEf+PvmBfn/0E+6BhSWRYwfHZtTTST5OwxPX06tWG1nFS+8zpja9JZf1abtCqWtcdC5UOUlwh5ngGuOwX/ioW2PqaVt+Nhcl82ST8ARgSZyQrhu2KP5WM2kjhjoNmtZ/65dIrUmrDZyw2q2j4lujf29sWlyr/APfUW4qfkdpfjMUu38o2SHa3gBLjo8PDj8KUMmjOUVwQOhde2O0MLXwIpkfwiY6ZoDXWxsDDPJ/TWWRJ3jHsWaItZsRdavgTT9q4IDTAO1RbjGex7x2Cani0It5ZMFdq8S70s8zixdihzbbPkCAwEAAaNCMEAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUCnSAG2vCQmlUfz/kI2Lq6SF9dxQwDgYDVR0PAQH/BAQDAgKEMA0GCSqGSIb3DQEBCwUAA4IBAQBIRgfisn4j+cze/i36vPUYgX98Bajn+TkjvFRFLuPTGoMavH8q3ndlGgaQJ6IvWjjefhMINtMQGJlOhbztGBRZQLd3MGmQe9MMErCMx+In8KJ671Qamf9TVd18faqZNINk5tKk+6T+rsFxeNoX33+Cgczb9jPNA0zbFTlIRtPrmyP0zhvkVynDc4xSzdwVkKFWjabigehzAmNhJEuFzdRLhotGi7DHZb3iA6eSBtwfLjFNVWup0+HXWXHJvR+sLSQdRTYkxsLxYLiASstAbJQ87YU6ysdLU4D5YRtdaN2/dQF0N1HlrJ6QCPmIMs0h+4eoprHaMQAPttKqLKzGHveY";

                var x509cert = new X509Certificate2(Convert.FromBase64String(publicCert));
                SecurityKey sk = new X509SecurityKey(x509cert);
                sk.KeyId = x509cert.Thumbprint;

                var basicConfig = new TokenValidationParameters()
                {
                    ValidIssuers = new List<string>() { "https://jusas-tests.eu.auth0.com/" },
                    ValidAudiences = new List<string>() { "http://localhost:7071/", "XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB" },
                    IssuerSigningKeys = new List<SecurityKey>() { sk },
                    ValidateIssuerSigningKey = true,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
                return new JwtAuthenticator(basicConfig);
            });
        }
    }
}

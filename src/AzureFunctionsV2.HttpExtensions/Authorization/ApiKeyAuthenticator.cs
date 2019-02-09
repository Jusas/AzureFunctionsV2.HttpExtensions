using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class ApiKeyAuthenticator : IApiKeyAuthenticator
    {
        private ApiKeyAuthenticationParameters _authenticationParameters;

        public ApiKeyAuthenticator(IOptions<HttpAuthenticationOptions> authOptions)
        {
            _authenticationParameters = authOptions?.Value?.ApiKeyAuthentication;
        }

        public async Task<bool> Authenticate(string apiKey, HttpRequest request)
        {
            if(_authenticationParameters == null)
                throw new InvalidOperationException("ApiKeyAuthenticationParameters have not been configured");

            return await _authenticationParameters.ApiKeyVerifier(apiKey, request);
        }
    }
}

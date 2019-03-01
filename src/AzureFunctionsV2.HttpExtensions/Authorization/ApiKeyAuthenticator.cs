using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// The API key authenticator.
    /// Contains the actual authentication logic.
    /// </summary>
    public class ApiKeyAuthenticator : IApiKeyAuthenticator
    {
        private readonly ApiKeyAuthenticationParameters _authenticationParameters;

        public ApiKeyAuthenticator(IOptions<HttpAuthenticationOptions> authOptions)
        {
            _authenticationParameters = authOptions?.Value?.ApiKeyAuthentication;
        }

        public async Task<bool> Authenticate(HttpRequest request)
        {
            if(_authenticationParameters?.ApiKeyVerifier == null)
                throw new InvalidOperationException("ApiKeyAuthenticationParameters ApiKeyVerifier has not been configured");

            if(string.IsNullOrEmpty(_authenticationParameters.HeaderName) && string.IsNullOrEmpty(_authenticationParameters.QueryParameterName))
                throw new InvalidOperationException("ApiKeyAuthenticationParameters HeaderName and QueryParameterName have neither been configured");

            var apiKeyQueryFieldName = _authenticationParameters.QueryParameterName;
            var apiKeyHeaderName = _authenticationParameters.HeaderName;

            if (!string.IsNullOrEmpty(apiKeyHeaderName))
            {
                if (request.Headers.ContainsKey(apiKeyHeaderName))
                    return await _authenticationParameters.ApiKeyVerifier(request.Headers[apiKeyHeaderName], request);
            }

            if (!string.IsNullOrEmpty(apiKeyQueryFieldName))
            {
                if (request.Query.ContainsKey(apiKeyQueryFieldName))
                    return await _authenticationParameters.ApiKeyVerifier(request.Query[apiKeyQueryFieldName], request);
            }

            return false;
        }
    }
}

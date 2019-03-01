using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// OAuth2 authenticator.
    /// </summary>
    public class OAuth2Authenticator : IOAuth2Authenticator
    {
        private readonly OAuth2AuthenticationParameters _authenticationParameters;

        public OAuth2Authenticator(IOptions<HttpAuthenticationOptions> httpAuthOptions)
        {
            _authenticationParameters = httpAuthOptions?.Value?.OAuth2Authentication;
        }

        public async Task<ClaimsPrincipal> AuthenticateAndAuthorize(string token, HttpRequest request, IList<HttpAuthorizeAttribute> attributes)
        {
            if (_authenticationParameters?.CustomAuthorizationFilter == null)
                throw new InvalidOperationException("OAuth2AuthenticationParameters have not been configured");
            
            if (!token.StartsWith("Bearer "))
                throw new HttpAuthenticationException("Expected Bearer token in Authorization header");
            token = token.Substring(7);

            return await _authenticationParameters.CustomAuthorizationFilter(token, request, attributes);
        }
    }
}

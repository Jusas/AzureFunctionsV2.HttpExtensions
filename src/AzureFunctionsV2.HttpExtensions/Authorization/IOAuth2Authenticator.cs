using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// The interface for the OAuth2 authenticator.
    /// </summary>
    public interface IOAuth2Authenticator
    {
        /// <summary>
        /// Authenticates and authorizes the user.
        /// <para>
        /// Note: requires the OAuth2AuthenticationParameters.CustomAuthorizationFilter set.
        /// Otherwise the implementation should throw, as the CustomAuthorizationFilter is mandatory
        /// from the point of authentication and authorization since the tokens can be anything.
        /// </para>
        /// </summary>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <param name="attributes"></param>
        /// <returns>The resolved ClaimsPrincipal.</returns>
        Task<ClaimsPrincipal> AuthenticateAndAuthorize(string token, HttpRequest request, IList<HttpAuthorizeAttribute> attributes);
    }
}

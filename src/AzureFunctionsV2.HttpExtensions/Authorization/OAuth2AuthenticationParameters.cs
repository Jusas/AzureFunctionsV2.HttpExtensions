using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class OAuth2AuthenticationParameters
    {
        /// <summary>
        /// An authorization method that must validate the token, given the HttpRequest and the authorization
        /// attributes. Returns the resolved ClaimsPrincipal, or must throw an exception if authorization failed
        /// or user was unauthorized.
        /// </summary>
        /// <param name="token">The authorization token</param>
        /// <param name="request">The HTTP request</param>
        /// <param name="authorizeAttributes">The Azure Function authorization attributes</param>
        /// <returns>The user ClaimsPrincipal</returns>
        /// <exception cref="HttpAuthenticationException"></exception>
        /// <exception cref="HttpAuthorizationException"></exception>
        public delegate Task<ClaimsPrincipal> AuthorizeMethod(string token, HttpRequest request,
            IList<HttpAuthorizeAttribute> authorizeAttributes);

        /// <summary>
        /// The authorization method that authenticates and authorizes the user.
        /// Returns the resolved ClaimsPrincipal, or must throw an exception if authorization failed
        /// or user was unauthorized.
        /// </summary>
        public AuthorizeMethod CustomAuthorizationFilter { get; set; }
    }
}

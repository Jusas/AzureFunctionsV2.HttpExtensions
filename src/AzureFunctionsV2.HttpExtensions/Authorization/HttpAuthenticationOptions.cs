using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Authentication options.
    /// Contains the options set for all different available types of authentication.
    /// </summary>
    public class HttpAuthenticationOptions
    {
        /// <summary>
        /// API key based authentication parameters.
        /// </summary>
        public ApiKeyAuthenticationParameters ApiKeyAuthentication { get; set; }

        /// <summary>
        /// Basic auth parameters.
        /// </summary>
        public BasicAuthenticationParameters BasicAuthentication { get; set; }

        /// <summary>
        /// OAuth2 based authentication parameters. Note: if you're configuring JWT based OAuth2 authentication,
        /// configure the JwtAuthentication property instead.
        /// </summary>
        public OAuth2AuthenticationParameters OAuth2Authentication { get; set; }

        /// <summary>
        /// JWT (JSON Web Token) based authentication parameters.
        /// </summary>
        public JwtAuthenticationParameters JwtAuthentication { get; set; }
    }
}
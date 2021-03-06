﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtAuthenticationParameters
    {
        /// <summary>
        /// Token validation parameters. These are used when validating JSON Web Tokens.
        /// <para>
        /// In practice you'll need to provide IssuerSigningKey(s), ValidIssuer(s) and ValidAudience(s).
        /// </para>
        /// <para>
        /// If you wish to use a OIDC conformant endpoint to populate the configuration (eg. Auth0), use
        /// the <see cref="OpenIdConnectJwtValidationParameters"/> class.
        /// </para>
        /// </summary>
        public TokenValidationParameters TokenValidationParameters { get; set; }

        /// <summary>
        /// A custom authorization filter. If provided, it will be used to authorize the user.
        /// Otherwise there will be no authorization applied, only authentication.
        /// This function should throw a <see cref="HttpAuthorizationException"/>
        /// if the authorization fails.
        /// </summary>
        public Func<ClaimsPrincipal, SecurityToken, IList<HttpAuthorizeAttribute>, Task> CustomAuthorizationFilter { get; set; }
    }
}

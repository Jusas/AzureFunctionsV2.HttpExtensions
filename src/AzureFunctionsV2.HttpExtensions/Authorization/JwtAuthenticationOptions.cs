using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtAuthenticationOptions
    {
        public TokenValidationParameters TokenValidationParameters { get; set; }

        /// <summary>
        /// A custom authorization filter. If provided, it will be used instead of the basic
        /// Claims checking. This function should throw a <see cref="HttpAuthorizationException"/>
        /// if the authorization fails.
        /// </summary>
        public Func<ClaimsPrincipal, SecurityToken, Task> CustomAuthorizationFilter { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class HttpAuthenticationOptions
    {
        public ApiKeyAuthenticationParameters ApiKeyAuthentication { get; set; }

        public BasicAuthenticationParameters BasicAuthentication { get; set; }

        public OAuth2AuthenticationParameters OAuth2Authentication { get; set; }

        public JwtAuthenticationParameters JwtAuthentication { get; set; }
    }
}
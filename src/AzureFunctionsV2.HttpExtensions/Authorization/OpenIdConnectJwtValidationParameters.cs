using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class OpenIdConnectJwtValidationParameters : TokenValidationParameters
    {
        public string OpenIdConnectConfigurationUrl { get; set; }
    }
}

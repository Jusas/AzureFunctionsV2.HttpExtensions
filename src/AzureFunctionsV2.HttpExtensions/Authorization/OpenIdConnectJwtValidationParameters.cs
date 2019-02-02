using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class OpenIdConnectJwtValidationParameters
    {
        public List<string> ValidAudiences { get; set; }
        public string OpenIdConnectConfigurationUrl { get; set; }

    }
}

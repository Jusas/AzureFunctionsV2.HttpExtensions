using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class BasicAuthenticationParameters
    {
        public Dictionary<string, string> ValidCredentials { get; set; } = new Dictionary<string, string>();
    }
}

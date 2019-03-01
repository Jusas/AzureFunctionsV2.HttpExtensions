using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// BasicAuth parameters.
    /// </summary>
    public class BasicAuthenticationParameters
    {
        /// <summary>
        /// A list of valid username, password pairs.
        /// </summary>
        public Dictionary<string, string> ValidCredentials { get; set; } = new Dictionary<string, string>();
    }
}

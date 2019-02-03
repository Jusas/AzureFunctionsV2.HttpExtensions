using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// TokenValidationParameters for using OpenIdConnect (OIDC) configuration.
    /// Use this with Auth0 for example.
    /// </summary>
    public class OpenIdConnectJwtValidationParameters : TokenValidationParameters
    {
        /// <summary>
        /// The configuration URL where the configuration will be fetched from.
        /// </summary>
        public string OpenIdConnectConfigurationUrl { get; set; }
    }
}

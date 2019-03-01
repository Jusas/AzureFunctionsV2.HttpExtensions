using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// JWT Authenticator implementation.
    /// Basically validates the JWT using the given validation parameters.    
    /// </summary>
    public class JwtAuthenticator : IJwtAuthenticator
    {
        private readonly TokenValidationParameters _jwtValidationParameters;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _manager;
        private readonly ISecurityTokenValidator _handler;

        public JwtAuthenticator(IOptions<HttpAuthenticationOptions> config, 
            ISecurityTokenValidator jwtSecurityTokenHandler,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            _jwtValidationParameters = config?.Value?.JwtAuthentication?.TokenValidationParameters;
            _handler = jwtSecurityTokenHandler;
            _manager = configurationManager;
        }

        /// <summary>
        /// Validates the token.
        /// Throws an exception if validation fails, otherwise returns the resolved <see cref="ClaimsPrincipal"/>
        /// and the validated token <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        /// <exception cref="HttpAuthorizationException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken)
        {
            if(_jwtValidationParameters == null)
                throw new InvalidOperationException("JwtAuthenticatorOptions have not been configured");

            if (!jwtToken.StartsWith("Bearer "))
                throw new HttpAuthenticationException("Expected Bearer token");
            jwtToken = jwtToken.Substring(7);

            try
            {
                if (_jwtValidationParameters is OpenIdConnectJwtValidationParameters oidcParams &&
                    _jwtValidationParameters.IssuerSigningKeys == null)
                {
                    var config = await _manager.GetConfigurationAsync(CancellationToken.None);
                    oidcParams.ValidIssuer = config.Issuer;
                    oidcParams.IssuerSigningKeys = config.SigningKeys;
                }

                var claimsPrincipal = _handler.ValidateToken(jwtToken, _jwtValidationParameters, out var validatedToken);
                return (claimsPrincipal, validatedToken);
            }
            catch (Exception e)
            {
                throw new HttpAuthenticationException("Token validation failed", e);
            }
        }
    }
}

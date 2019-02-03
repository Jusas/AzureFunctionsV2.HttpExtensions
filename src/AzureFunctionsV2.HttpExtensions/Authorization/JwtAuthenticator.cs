using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtAuthenticator : IJwtAuthenticator
    {
        private TokenValidationParameters _jwtValidationParameters;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _manager;
        private ISecurityTokenValidator _handler;

        public JwtAuthenticator(IOptions<JwtAuthenticatorOptions> config, 
            ISecurityTokenValidator jwtSecurityTokenHandler,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            _jwtValidationParameters = config?.Value.TokenValidationParameters;
            _handler = jwtSecurityTokenHandler;
            _manager = configurationManager;
        }

        public async Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken)
        {
            if(_jwtValidationParameters == null)
                throw new InvalidOperationException("JwtAuthenticatorOptions have not been configured");

            if (_jwtValidationParameters is OpenIdConnectJwtValidationParameters oidcParams &&
                _jwtValidationParameters.IssuerSigningKeys == null)
            {
                var config = await _manager.GetConfigurationAsync(CancellationToken.None);
                oidcParams.ValidIssuer = config.Issuer;
                oidcParams.IssuerSigningKeys = config.SigningKeys;
            }

            try
            {
                SecurityToken validatedToken = null;
                var claimsPrincipal = _handler.ValidateToken(jwtToken, _jwtValidationParameters, out validatedToken);
                return (claimsPrincipal, validatedToken);
            }
            catch (Exception e)
            {
                throw new HttpAuthenticationException("Token validation failed", e);
            }
        }
    }
}

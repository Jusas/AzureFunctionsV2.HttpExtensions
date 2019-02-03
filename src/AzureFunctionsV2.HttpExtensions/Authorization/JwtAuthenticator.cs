using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtAuthenticator : IJwtAuthenticator
    {
        private TokenValidationParameters _jwtValidationParameters;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _manager;
        private JwtSecurityTokenHandler _handler;

        public JwtAuthenticator(TokenValidationParameters parameters)
        {
            _jwtValidationParameters = parameters;
            _handler = new JwtSecurityTokenHandler();
            if(parameters is OpenIdConnectJwtValidationParameters oidcParams)
                _manager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    oidcParams.OpenIdConnectConfigurationUrl,
                    new OpenIdConnectConfigurationRetriever());
        }

        public async Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken)
        {
            if (_jwtValidationParameters is OpenIdConnectJwtValidationParameters oidcParams &&
                _jwtValidationParameters.IssuerSigningKeys == null)
            {
                var config = await _manager.GetConfigurationAsync();
                oidcParams.ValidIssuer = config.Issuer;
                oidcParams.IssuerSigningKeys = config.SigningKeys;
            }

            try
            {
                SecurityToken validatedToken;
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

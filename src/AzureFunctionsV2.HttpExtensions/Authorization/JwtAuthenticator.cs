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
        private OpenIdConnectJwtValidationParameters _oidcJwtValidationParameters;
        private JwtValidationParameters _jwtValidationParameters;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _manager;
        private JwtSecurityTokenHandler _handler;

        public JwtAuthenticator(OpenIdConnectJwtValidationParameters parameters)
        {
            _oidcJwtValidationParameters = parameters;
            _handler = new JwtSecurityTokenHandler();
            _manager = new ConfigurationManager<OpenIdConnectConfiguration>(
                parameters.OpenIdConnectConfigurationUrl,
                new OpenIdConnectConfigurationRetriever());
        }

        public JwtAuthenticator(JwtValidationParameters parameters)
        {
            _jwtValidationParameters = parameters;
            _handler = new JwtSecurityTokenHandler();
        }

        public async Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken)
        {
            SecurityToken validatedToken;
            TokenValidationParameters parameters;
            if (_oidcJwtValidationParameters != null)
            {
                var config = await _manager.GetConfigurationAsync();
                parameters = new TokenValidationParameters() // TODO: don't abstract this behind _oidcJwtValidationParameters?
                {
                    ValidAudiences = _oidcJwtValidationParameters.ValidAudiences,
                    ValidIssuers = new List<string>() {config.Issuer},
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = config.SigningKeys,
                    NameClaimType = ClaimTypes.NameIdentifier
                };                
            }
            else
            {
                parameters = new TokenValidationParameters()
                {
                    ValidAudiences = _jwtValidationParameters.ValidAudiences,
                    ValidIssuers = _jwtValidationParameters.ValidIssuers,
                    ValidateIssuerSigningKey = _jwtValidationParameters.IssuerSigningKeys != null &&
                                               _jwtValidationParameters.IssuerSigningKeys.Any()
                        ? true
                        : false,
                    IssuerSigningKeys = _jwtValidationParameters.IssuerSigningKeys,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            }

            try
            {
                var claimsPrincipal = _handler.ValidateToken(jwtToken, parameters, out validatedToken);
                return (claimsPrincipal, validatedToken);
            }
            catch (Exception e)
            {
                throw new HttpAuthenticationException("Token validation failed", e);
            }
        }
    }
}

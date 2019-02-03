﻿using System;
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
        private TokenValidationParameters _jwtValidationParameters;
        private readonly IConfigurationManager<OpenIdConnectConfiguration> _manager;
        private ISecurityTokenValidator _handler;

        public JwtAuthenticator(IOptions<JwtAuthenticationOptions> config, 
            ISecurityTokenValidator jwtSecurityTokenHandler,
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            _jwtValidationParameters = config?.Value.TokenValidationParameters;
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
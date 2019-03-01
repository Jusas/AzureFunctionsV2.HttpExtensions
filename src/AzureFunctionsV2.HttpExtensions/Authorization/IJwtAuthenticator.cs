using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// The interface for a JWT Authenticator.
    /// The JWT Authenticator verifies that the JWT contents are valid and returns
    /// claims and the validated token.
    /// </summary>
    public interface IJwtAuthenticator
    {
        /// <summary>
        /// The authentication method.
        /// </summary>
        /// <param name="jwtToken">JWT token to validate.</param>
        /// <returns>The ClaimsPrincipal and SecurityToken resulting from the operation if it succeeded.</returns>
        /// <exception cref="HttpAuthorizationException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken);
    }
}

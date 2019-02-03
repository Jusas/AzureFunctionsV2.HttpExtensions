using System.Security.Claims;
using System.Threading.Tasks;
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
        Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken);
    }
}

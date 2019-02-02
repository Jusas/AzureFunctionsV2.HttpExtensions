using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public interface IJwtAuthenticator
    {
        Task<(ClaimsPrincipal claimsPrincipal, SecurityToken validatedToken)> Authenticate(string jwtToken);
    }
}

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public interface IOAuth2Authenticator
    {
        Task<ClaimsPrincipal> Authenticate(string token, HttpRequest request, IList<HttpAuthorizeAttribute> attributes);
    }
}

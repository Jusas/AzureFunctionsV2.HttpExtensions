using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using AzureFunctionsV2.HttpExtensions.Annotations;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class HttpUser
    {
        public ClaimsPrincipal ClaimsPrincipal { get; set; }
        public static implicit operator ClaimsPrincipal(HttpUser user)
        {
            return user.ClaimsPrincipal;
        }
    }
}

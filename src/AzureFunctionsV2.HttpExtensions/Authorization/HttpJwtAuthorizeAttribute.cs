using System;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class HttpJwtAuthorizeAttribute : Attribute
    {
        public HttpJwtAuthorizeAttribute()
        {

        }

        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}

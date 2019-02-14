using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public enum Scheme
    {
        Basic,
        OAuth2,
        Jwt,
        HeaderApiKey,
        QueryApiKey
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpAuthorizeAttribute : Attribute
    {
        public HttpAuthorizeAttribute(Scheme scheme)
        {
            Scheme = scheme;
        }

        public HttpAuthorizeAttribute()
        {
        }

        public Scheme Scheme { get; set; }
    }
}

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

    /// <summary>
    /// Attribute that indicates that the Function requires authentication/authorization.
    /// </summary>
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

        /// <summary>
        /// The required authentication scheme.
        /// </summary>
        public Scheme Scheme { get; set; }
    }
}

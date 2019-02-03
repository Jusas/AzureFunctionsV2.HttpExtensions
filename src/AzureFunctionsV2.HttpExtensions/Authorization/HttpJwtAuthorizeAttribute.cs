using System;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Method attribute that signifies that the method requires
    /// authentication and/or authorization with a JSON Web Token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)] // TODO: support multiple attributes
    public class HttpJwtAuthorizeAttribute : Attribute
    {
        public HttpJwtAuthorizeAttribute()
        {

        }

        /// <summary>
        /// Claim type that you require to be present in the JWT. This claim must be present in the token,
        /// or the authorization will fail.
        /// <para>
        /// If left as null, no claim will be required.
        /// </para>
        /// </summary>
        public string ClaimType { get; set; }

        /// <summary>
        /// Value of the claim that must be present for the given claim type. If the values do not match,
        /// the authorization will fail.
        /// </summary>
        public string ClaimValue { get; set; }
    }
}

using System;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Method attribute that signifies that the method requires
    /// authentication and/or authorization with a JSON Web Token.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HttpJwtAuthorizeAttribute : Attribute
    {
        public HttpJwtAuthorizeAttribute()
        {
        }
    }
}

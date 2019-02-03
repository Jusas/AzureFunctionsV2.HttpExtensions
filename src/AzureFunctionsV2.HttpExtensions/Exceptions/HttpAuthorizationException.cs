using System;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    /// <summary>
    /// The exception is thrown when authorization checks fail and authorization is required.
    /// </summary>
    public class HttpAuthorizationException : Exception
    {
        public HttpAuthorizationException(string message, Exception inner = null) : base(message, inner)
        {
        }
    }
}

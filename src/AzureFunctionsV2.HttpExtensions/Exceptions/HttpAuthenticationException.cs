using System;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    /// <summary>
    /// The exception is thrown when authentication checks fail and authentication is required.
    /// </summary>
    public class HttpAuthenticationException : Exception
    {
        public HttpAuthenticationException(string message, Exception inner = null) : base(message, inner)
        {
        }
    }
}

using System;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    /// <summary>
    /// The parent class for custom exceptions.
    /// </summary>
    public abstract class HttpExtensionsException : Exception
    {
        public HttpContext HttpContext { get; }
        public string ParameterName { get; }

        protected HttpExtensionsException(string message, Exception inner, string parameterName, HttpContext requestHttpContext) : base(message, inner)
        {
            HttpContext = requestHttpContext;
            ParameterName = parameterName;
        }
    }
}

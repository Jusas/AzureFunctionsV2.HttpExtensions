using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    public abstract class HttpExtensionsException : Exception
    {
        public HttpContext HttpContext { get; }
        public string ParameterName { get; }

        public HttpExtensionsException(string message, Exception inner, string parameterName, HttpContext requestHttpContext) : base(message, inner)
        {
            HttpContext = requestHttpContext;
            ParameterName = parameterName;
        }
    }
}

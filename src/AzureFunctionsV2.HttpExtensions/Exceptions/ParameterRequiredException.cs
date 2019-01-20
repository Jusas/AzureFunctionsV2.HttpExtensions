using System;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    /// <summary>
    /// The exception is thrown when a <see cref="HttpParam{T}"/> is marked as required by a <see cref="HttpSourceAttribute"/>
    /// but the parameter was not present in the request or was null/empty.
    /// </summary>
    public class ParameterRequiredException : HttpExtensionsException
    {
        public ParameterRequiredException(string message, Exception inner, string parameterName, HttpContext requestHttpContext) : base(message, inner, parameterName, requestHttpContext)
        {
            
        }
    }
}

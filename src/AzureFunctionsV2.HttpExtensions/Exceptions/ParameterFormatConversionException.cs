using System;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    /// <summary>
    /// The exception is thrown when there was an error converting values when trying to assign
    /// values to <see cref="HttpParam{T}"/> parameters.
    /// </summary>
    public class ParameterFormatConversionException : HttpExtensionsException
    {
        public ParameterFormatConversionException(string message, Exception inner, string parameterName, HttpContext requestHttpContext) : base(message, inner, parameterName, requestHttpContext)
        {
        }
    }
}

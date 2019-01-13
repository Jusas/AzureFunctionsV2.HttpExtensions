using System;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    public class FormParameterException : HttpExtensionsException
    {
        public FormParameterException(string message, Exception inner, string parameterName, HttpContext requestHttpContext) : base(message, inner, parameterName, requestHttpContext)
        {
        }
    }
}

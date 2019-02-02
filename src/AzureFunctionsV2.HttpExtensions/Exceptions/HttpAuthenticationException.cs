using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Exceptions
{
    public class HttpAuthenticationException : Exception
    {
        public HttpAuthenticationException(string message, Exception inner = null) : base(message, inner)
        {
        }
    }
}

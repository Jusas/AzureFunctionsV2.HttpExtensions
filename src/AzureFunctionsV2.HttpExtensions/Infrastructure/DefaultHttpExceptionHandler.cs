using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class DefaultHttpExceptionHandler : IHttpExceptionHandler
    {
        protected IHttpResponseErrorFormatter _errorFormatter;

        public DefaultHttpExceptionHandler(IHttpResponseErrorFormatter errorFormatter)
        {
            _errorFormatter = errorFormatter;
        }

        public async Task HandleException(FunctionExceptionContext exceptionContext, HttpContext httpContext)
        {
            httpContext.Response.OnStarting(async () =>
                {
                    await _errorFormatter.WriteErrorResponse(exceptionContext, httpContext.Response);
                });
        }
    }
}

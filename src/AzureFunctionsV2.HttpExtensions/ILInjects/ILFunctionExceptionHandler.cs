using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Web.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.ILInjects
{
    public class ILFunctionExceptionHandler : IILFunctionExceptionHandler
    {

        private static IHttpExceptionHandler _httpExceptionHandler;

        public ILFunctionExceptionHandler(IHttpExceptionHandler httpExceptionHandler)
        {
            _httpExceptionHandler = httpExceptionHandler;
        }

        public static void RethrowStoredException(HttpRequest request)
        {
            // Simply rethrow a stored exception if one exists.
            if (request.HttpContext.Items.ContainsKey(nameof(HttpParamAssignmentFilter)))
            {
                var exception =
                    request.HttpContext.Items.First(item => (string)item.Key == nameof(HttpParamAssignmentFilter)).Value as
                        Exception;
                if (exception != null)
                    throw exception;
            }
            if (request.HttpContext.Items.ContainsKey(nameof(HttpAuthorizationFilter)))
            {
                var exception =
                    request.HttpContext.Items.First(item => (string)item.Key == nameof(HttpAuthorizationFilter)).Value as
                        Exception;
                if (exception != null)
                    throw exception;
            }
        }

        public static IActionResult HandleExceptionAndReturnResult(Exception exception, HttpRequest request)
        {
            if (_httpExceptionHandler != null)
            {
                var awaitable = _httpExceptionHandler.HandleException(
                    (FunctionExecutingContext) request.HttpContext.Items["FunctionExecutingContext"],
                    request, exception);
                return awaitable.Result;
            }

            return new InternalServerErrorResult();
        }
    }
}

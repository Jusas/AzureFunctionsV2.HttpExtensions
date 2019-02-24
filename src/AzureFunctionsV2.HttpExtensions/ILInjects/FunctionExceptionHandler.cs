using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsV2.HttpExtensions.ILInjects
{
    public static class FunctionExceptionHandler
    {
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

        public static IActionResult HandleExceptionAndReturnResult(Exception e)
        {
            return new OkObjectResult(e);
        }
    }
}

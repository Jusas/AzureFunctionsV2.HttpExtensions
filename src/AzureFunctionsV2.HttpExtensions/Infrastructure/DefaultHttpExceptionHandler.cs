using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class DefaultHttpExceptionHandler : IHttpExceptionHandler
    {
        public static bool OutputRecursiveExceptionMessages { get; set; }

        public DefaultHttpExceptionHandler()
        {
        }
        
        protected virtual string GetExceptionMessageRecursive(Exception outermostException)
        {
            var messages = new List<string>();
            var exception = outermostException;
            while (exception != null)
            {
                messages.Add(exception.Message);
                exception = exception.InnerException;
            }

            return string.Join("; ", messages);
        }

        public virtual async Task<IActionResult> HandleException(FunctionExecutingContext functionExecutingContext, HttpRequest request, Exception exception)
        {
            functionExecutingContext?.Logger.LogError(exception, GetExceptionMessageRecursive(exception));

            var errorObject = new Dictionary<string, string>();

            var httpExtensionsException = exception as HttpExtensionsException
                                          ?? exception.InnerException as HttpExtensionsException;

            if (httpExtensionsException is ParameterFormatConversionException ||
                httpExtensionsException is ParameterRequiredException)
            {
                var response = new BadRequestObjectResult(errorObject);
                errorObject.Add("message", OutputRecursiveExceptionMessages
                    ? GetExceptionMessageRecursive(httpExtensionsException)
                    : httpExtensionsException.Message);
                errorObject.Add("parameter", httpExtensionsException.ParameterName);
                return response;
            }

            var httpAuthenticationException = exception as HttpAuthenticationException
                                          ?? exception.InnerException as HttpAuthenticationException;

            if (httpAuthenticationException != null)
            {
                var response = new ObjectResult(errorObject);
                response.StatusCode = 401;
                errorObject.Add("message", OutputRecursiveExceptionMessages
                    ? GetExceptionMessageRecursive(httpAuthenticationException)
                    : httpAuthenticationException.Message);
                return response;
            }

            var httpAuthorizationException = exception as HttpAuthorizationException
                                              ?? exception.InnerException as HttpAuthorizationException;

            if (httpAuthorizationException != null)
            {
                var response = new ObjectResult(errorObject);
                response.StatusCode = 403;
                errorObject.Add("message", OutputRecursiveExceptionMessages
                    ? GetExceptionMessageRecursive(httpAuthorizationException)
                    : httpAuthorizationException.Message);
                return response;
            }

            var defaultResponse = new ObjectResult(errorObject);
            defaultResponse.StatusCode = 500;
            errorObject.Add("message", OutputRecursiveExceptionMessages
                ? GetExceptionMessageRecursive(exception)
                : exception.Message);
            return defaultResponse;
        }
    }
}

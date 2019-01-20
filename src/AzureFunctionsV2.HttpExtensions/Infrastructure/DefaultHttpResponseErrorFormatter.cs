using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// <para>
    /// The default implementation for <see cref="IHttpResponseErrorFormatter"/>.
    /// Returns StatusCode 400 for specific <see cref="HttpExtensionsException"/> types when the
    /// result can be interpreted as Bad Request. In all other cases, returns 500.
    /// </para>
    /// <para>
    /// Writes a JSON serialized dictionary to the response body containing "message" string and in the case of
    /// Bad Request also a "parameter" string that indicates the parameter that caused the exception.
    /// </para>
    /// <para>
    /// <example>Response example:
    /// <code>{"message": "Something went wrong", "parameter": "firstname"}</code>
    /// </example>
    /// </para>
    /// </summary>
    public class DefaultHttpResponseErrorFormatter : IHttpResponseErrorFormatter
    {
        public static bool OutputRecursiveExceptionMessages { get; set; } = true;

        public static string GetExceptionMessageRecursive(Exception outermostException)
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

        public async Task WriteErrorResponse(FunctionExceptionContext exceptionContext, HttpResponse response)
        {
            var errorObject = new Dictionary<string, string>();

            var httpExtensionsException = exceptionContext.Exception as HttpExtensionsException 
                                          ?? exceptionContext.Exception.InnerException as HttpExtensionsException;

            if (httpExtensionsException != null)
            {
                if (httpExtensionsException is ParameterFormatConversionException ||
                    httpExtensionsException is ParameterRequiredException)
                {
                    response.StatusCode = 400;
                    response.Headers.Add("Content-Type", "application/json");
                    errorObject.Add("message", OutputRecursiveExceptionMessages 
                        ? GetExceptionMessageRecursive(httpExtensionsException) 
                        : httpExtensionsException.Message);
                    errorObject.Add("parameter", httpExtensionsException.ParameterName);
                    await response.WriteAsync(JsonConvert.SerializeObject(errorObject));
                }

                return;
            }

            response.StatusCode = 500;
            response.Headers.Add("Content-Type", "application/json");
            errorObject.Add("message", OutputRecursiveExceptionMessages 
                ? GetExceptionMessageRecursive(exceptionContext.Exception) 
                : exceptionContext.Exception.Message);
            await response.WriteAsync(JsonConvert.SerializeObject(errorObject));
        }
    }
}

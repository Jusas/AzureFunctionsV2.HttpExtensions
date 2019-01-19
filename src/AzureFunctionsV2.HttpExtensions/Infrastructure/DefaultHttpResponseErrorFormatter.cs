using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class DefaultHttpResponseErrorFormatter : IHttpResponseErrorFormatter
    {
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
                    errorObject.Add("message", httpExtensionsException.Message);
                    errorObject.Add("parameter", httpExtensionsException.ParameterName);
                    await response.WriteAsync(JsonConvert.SerializeObject(errorObject));
                }

                return;
            }

            response.StatusCode = 500;
            response.Headers.Add("Content-Type", "application/json");
            errorObject.Add("message", exceptionContext.Exception.Message);
            await response.WriteAsync(JsonConvert.SerializeObject(errorObject));
        }
    }
}

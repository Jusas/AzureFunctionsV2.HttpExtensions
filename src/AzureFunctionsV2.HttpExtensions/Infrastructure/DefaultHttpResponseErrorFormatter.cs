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
            var httpExtensionsException = exceptionContext.Exception as HttpExtensionsException;
            if (httpExtensionsException == null)
            {
                httpExtensionsException = exceptionContext.Exception.InnerException as HttpExtensionsException;
            }

            if (httpExtensionsException != null)
            {
                if (httpExtensionsException is ParameterFormatConversionException ||
                    httpExtensionsException is ParameterRequiredException)
                {
                    response.OnStarting(async () =>
                    {
                        response.StatusCode = 400;
                        response.Headers.Add("Content-Type", "application/json");
                        var errorObject = new
                        {
                            message = httpExtensionsException.Message,
                            parameter = httpExtensionsException.ParameterName
                        };
                        var json = JsonConvert.SerializeObject(errorObject);
                        await response.WriteAsync(json);
                    });
                }

                return;
            }

            response.OnStarting(async () =>
            {
                response.StatusCode = 500;
                response.Headers.Add("Content-Type", "application/json");
                var errorObject = new
                {
                    message = exceptionContext.Exception.Message
                };
                var json = JsonConvert.SerializeObject(errorObject);
                await response.WriteAsync(json);
            });
        }
    }
}

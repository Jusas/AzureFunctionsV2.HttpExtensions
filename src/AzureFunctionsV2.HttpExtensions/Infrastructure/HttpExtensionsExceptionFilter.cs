using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// Exception filter to enable customizing responses when the Function throws
    /// an exception. Invokes the registered <see cref="IHttpExceptionHandler"/> implementation.
    /// </summary>
    public class HttpExtensionsExceptionFilter : IFunctionExceptionFilter
    {
        private static IHttpExceptionHandler _httpExceptionHandler;
        private IHttpRequestStore _httpRequestStore;

        public HttpExtensionsExceptionFilter(IHttpRequestStore httpRequestStore, IHttpExceptionHandler httpExceptionHandler)
        {
            _httpRequestStore = httpRequestStore;
            _httpExceptionHandler = httpExceptionHandler;
        }

        public async Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            exceptionContext.Logger.LogError(exceptionContext.Exception.ToString());
            var httpContext = _httpRequestStore.Get(exceptionContext.FunctionInstanceId)?.HttpContext;
            if(_httpExceptionHandler != null)
                await _httpExceptionHandler.HandleException(exceptionContext, httpContext);
            _httpRequestStore.Remove(exceptionContext.FunctionInstanceId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.IL;
using AzureFunctionsV2.HttpExtensions.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// This is a FunctionFilter whose whole purpose is to store capture some metadata from
    /// the request for the duration of the function invocation for other components to use.
    /// </summary>
    public class HttpRequestMetadataStorageFilter : IFunctionInvocationFilter
    {
        private IHttpRequestStore _httpRequestStore;

        public HttpRequestMetadataStorageFilter(IHttpRequestStore httpRequestStore,
            IILFunctionExceptionHandler ilFunctionExceptionHandler /* just so that ILFunctionExceptionHandler gets initialized */)
        {
            _httpRequestStore = httpRequestStore;
        }

        public async Task OnExecutingAsync(FunctionExecutingContext executingContext,
            CancellationToken cancellationToken)
        {
            if (executingContext.Arguments.Values.Where(x => x != null).FirstOrDefault(
                x => typeof(HttpRequest).IsAssignableFrom(x.GetType())) is HttpRequest httpRequest)
            {
                _httpRequestStore.Set(executingContext.FunctionInstanceId, httpRequest);
                httpRequest.HttpContext.StoreFunctionExecutingContext(executingContext);
            }
        }

        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            _httpRequestStore.Remove(executedContext.FunctionInstanceId);
        }
    }
}

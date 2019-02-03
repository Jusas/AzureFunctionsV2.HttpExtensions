using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// The default implementation of <see cref="IHttpExceptionHandler"/>.
    /// Invokes the <see cref="IHttpResponseErrorFormatter"/> upon calling HandleException.
    /// </summary>
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

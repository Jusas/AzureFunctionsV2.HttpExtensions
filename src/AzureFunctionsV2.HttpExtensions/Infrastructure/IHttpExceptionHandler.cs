using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// Implementing this interface and registering this to the service collection as singleton from startup code:
    /// <example>builder.Services.Replace(ServiceDescriptor.Singleton&lt;IHttpExceptionHandler, TImplementation&gt;())</example>
    /// will use the exception handler when an exception is thrown inside the Function.
    /// By default the <see cref="DefaultHttpExceptionHandler"/> is used.
    /// </summary>
    public interface IHttpExceptionHandler
    {
        /// <summary>
        /// Handles the exception. Can for example write to the httpContext.Response (note: use httpContext.Response.OnStarting() for that).
        /// </summary>
        /// <param name="exceptionContext">The function exception context</param>
        /// <param name="httpContext">The request HTTP context</param>
        /// <returns></returns>
        Task HandleException(FunctionExceptionContext exceptionContext, HttpContext httpContext);
    }
}

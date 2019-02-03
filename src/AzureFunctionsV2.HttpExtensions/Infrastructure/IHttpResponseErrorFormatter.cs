using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// Interface for response error formatter.
    /// Implementing this interface and registering this to the service collection as singleton from startup code:
    /// <example>builder.Services.Replace(ServiceDescriptor.Singleton&lt;IHttpResponseErrorFormatter, TImplementation&gt;()</example>
    /// will use the error formatter when writing error responses. By default the <see cref="DefaultHttpResponseErrorFormatter"/>
    /// is used.
    /// </summary>
    public interface IHttpResponseErrorFormatter
    {
        Task WriteErrorResponse(FunctionExceptionContext exceptionContext, HttpResponse response);
    }
}

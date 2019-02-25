using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// </summary>
    public interface IHttpExceptionHandler
    {
        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <returns></returns>
        Task<IActionResult> HandleException(FunctionExecutingContext functionExecutingContext, HttpRequest request, Exception exception);
    }
}

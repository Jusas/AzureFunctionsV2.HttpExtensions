using System;
using System.Linq;
using System.Web.Http;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureFunctionsV2.HttpExtensions.IL
{
    /// <summary>
    /// The exception handler methods whose invocation is injected to the IL assembly code
    /// after compilation (to each Function and its compiler generated async state machine method).
    /// </summary>
    public class ILFunctionExceptionHandler : IILFunctionExceptionHandler
    {

        private static IHttpExceptionHandler _httpExceptionHandler;

        public ILFunctionExceptionHandler(IHttpExceptionHandler httpExceptionHandler)
        {
            _httpExceptionHandler = httpExceptionHandler;
        }

        /// <summary>
        /// A method that simply looks for stored exceptions from the request's HttpContext
        /// and throws the first one found if one exists.
        /// <para>
        /// This method is called at the beginning of each HTTP triggered Function; its job
        /// is to rethrow the exceptions captured inside the Function Filters because they
        /// can't throw immediately, otherwise it'd cause an unhandled exception and we'd
        /// lose control of the Function return value.
        /// </para>
        /// </summary>
        /// <param name="request"></param>
        public static void RethrowStoredException(HttpRequest request)
        {
            // Simply rethrow a stored exception if one exists.
            var exception = request.HttpContext.GetStoredExceptions().FirstOrDefault();
            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// A method that is called in the async state machine's MoveNext() catch block.
        /// This effectively replaces the compiler generated SetException() call,
        /// and enables us to swallow and handle the exception and return a valid value
        /// from the Function - enabling us to return custom error results.
        /// </summary>
        /// <param name="exception">The thrown exception</param>
        /// <param name="request"></param>
        /// <returns>Either InternalServerErrorResult, or any IActionResult if the _httpExceptionHandler is defined.
        /// By default it should be set to <see cref="DefaultHttpExceptionHandler"/>.</returns>
        public static IActionResult HandleExceptionAndReturnResult(Exception exception, HttpRequest request)
        {
            if (_httpExceptionHandler != null)
            {
                var awaitable = _httpExceptionHandler.HandleException(
                    request.HttpContext.GetStoredFunctionExecutingContext(),
                    request, exception);
                return awaitable.Result;
            }

            return new InternalServerErrorResult();
        }
    }
}

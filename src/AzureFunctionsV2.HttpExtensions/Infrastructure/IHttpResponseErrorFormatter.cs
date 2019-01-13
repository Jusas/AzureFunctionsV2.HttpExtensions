using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public interface IHttpResponseErrorFormatter
    {
        Task WriteErrorResponse(FunctionExceptionContext exceptionContext, HttpResponse response);
    }
}

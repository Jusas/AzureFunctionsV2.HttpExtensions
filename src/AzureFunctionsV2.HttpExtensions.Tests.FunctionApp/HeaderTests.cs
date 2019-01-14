using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp
{
    public static class HeaderTests
    {
        [FunctionName("HeaderTestFunction-Primitives")]
        public static async Task<IActionResult> HeaderTest_Primitives(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpHeader("x-my-string-header")]HttpParam<string> stringHeader,
            [HttpHeader("x-my-int-header")]HttpParam<int> intHeader,
            [HttpHeader("x-my-enum-header")]HttpParam<TestEnum> enumHeader,
            ILogger log)
        {
            return new OkObjectResult(new HeaderTestResultSet()
            {
                EnumParam = enumHeader,
                IntParam = intHeader,
                StringParam = stringHeader
            });
        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Examples.FunctionApp
{
    public static class HeaderParameters
    {

        /*
        GET http://localhost:7071/api/header-basics
        x-my-header: hello world
        x-my-json-header: {"name": "John"}

        */
        /// <summary>
        /// The basics of headers.
        /// Headers support strings, as the headers normally are strings, but
        /// also deserialization of JSON objects (excluding root level arrays)
        /// are supported as shown below.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="myHeader"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("HeaderParametersDemo1")]
        public static async Task<IActionResult> HeaderParametersDemo1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "header-basics")] HttpRequest req,
            [HttpHeader(Name = "x-my-header")]HttpParam<string> myBasicHeader,
            [HttpHeader(Name = "x-my-json-header")]HttpParam<BodyParameters.MyObject> myJsonHeader,
            ILogger log)
        {
            log.LogInformation($"x-my-header: {myBasicHeader}");
            log.LogInformation($"x-my-json-header: {JsonConvert.SerializeObject(myJsonHeader.Value)}");
            return new OkObjectResult("see the log");
        }
    }
}

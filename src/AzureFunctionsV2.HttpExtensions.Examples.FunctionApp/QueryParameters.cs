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
    public static class QueryParameters
    {
        /*
        GET http://localhost:7071/api/query-basics?someString=hello
          &anotherString=world&myObject={"Name": "John"}
          &numberArray=1&numberArray=2
          &stringList=foo&stringList=bar 
        */
        /// <summary>
        /// The basics of HttpQueryAttribute usage.
        /// This is somewhat self explanatory. Objects are deserialized, and
        /// application/xml and application/json are supported with default settings
        /// out of the box. The same applies to Arrays and Lists.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="someString"></param>
        /// <param name="yetAnother"></param>
        /// <param name="myObject"></param>
        /// <param name="numberArray"></param>
        /// <param name="stringList"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("QueryParametersDemo1")]
        public static async Task<IActionResult> QueryParametersDemo1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "query-basics")] HttpRequest req,
            [HttpQuery(Required = true)]HttpParam<string> someString,
            [HttpQuery(Name = "anotherString")]HttpParam<string> yetAnother,
            [HttpQuery]HttpParam<BodyParameters.MyObject> myObject,
            [HttpQuery]HttpParam<long[]> numberArray,
            [HttpQuery]HttpParam<List<string>> stringList,
            ILogger log)
        {
            log.LogInformation($"someString: {someString}");
            log.LogInformation($"anotherString: {yetAnother}");
            log.LogInformation($"myObject: {JsonConvert.SerializeObject(myObject.Value)}");
            log.LogInformation($"numberArray: {JsonConvert.SerializeObject(numberArray.Value)}");
            log.LogInformation($"stringList: {JsonConvert.SerializeObject(stringList.Value)}");
            return new OkObjectResult("see the log");
        }
    }
}

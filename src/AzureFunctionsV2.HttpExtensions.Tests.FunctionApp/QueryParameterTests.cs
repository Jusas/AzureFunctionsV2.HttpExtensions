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
    public static class QueryParameterTests
    {
        [FunctionName("QueryParameterTestFunction-Basic")]
        public static async Task<IActionResult> QueryParameterTestFunction_Basic(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpQuery(Required = true)]HttpParam<string> stringParam,
            [HttpQuery]HttpParam<int> intParam,
            [HttpQuery]HttpParam<TestEnum> enumParam,
            ILogger log)
        {
            return new OkObjectResult(new QueryTestResultSet()
            {
                EnumParam = enumParam,
                IntParam = intParam,
                StringParam = stringParam
            });
        }

        [FunctionName("QueryParameterTestFunction-Arrays")]
        public static async Task<IActionResult> QueryParameterTestFunction_Arrays(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpQuery(Required = true)]HttpParam<string[]> stringArray,
            [HttpQuery]HttpParam<int[]> intArray,
            [HttpQuery]HttpParam<TestEnum[]> enumArray,
            ILogger log)
        {
            return new OkObjectResult(new QueryArrayTestResultSet()
            {
                EnumArray = enumArray,
                IntArray = intArray,
                StringArray = stringArray
            });
        }

        [FunctionName("QueryParameterTestFunction-Lists")]
        public static async Task<IActionResult> QueryParameterTestFunction_Lists(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpQuery(Required = true)]HttpParam<List<string>> stringList,
            [HttpQuery]HttpParam<List<int>> intList,
            [HttpQuery]HttpParam<List<TestEnum>> enumList,
            ILogger log)
        {
            return new OkObjectResult(new QueryListTestResultSet()
            {
                EnumList = enumList,
                IntList = intList,
                StringList = stringList
            });
        }

    }
}

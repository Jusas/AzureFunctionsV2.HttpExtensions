using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp
{
    public static class AuthTests
    {
        /// <summary>
        /// Function with HttpJwtAuthorize, with no claims.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest1")]
        [HttpJwtAuthorize]
        public static async Task<IActionResult> JwtAuthTest1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpJwt]HttpUser user,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Function without HttpJwtAuthorize.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest3")]
        public static async Task<IActionResult> JwtAuthTest3(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpJwt]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }
    }
}

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
        [HttpAuthorize(Scheme.Jwt)]
        public static async Task<IActionResult> JwtAuthTest1(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkResult();
        }

        /// <summary>
        /// Authorization with Basic Auth.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest2")]
        [HttpAuthorize(Scheme.Basic)]
        public static async Task<IActionResult> JwtAuthTest2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
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
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        /// <summary>
        ///  Authorization with OAuth2 token
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest4")]
        [HttpAuthorize(Scheme.OAuth2)]
        public static async Task<IActionResult> JwtAuthTest4(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        /// <summary>
        ///  Authorization with HeaderApiKey
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest5")]
        [HttpAuthorize(Scheme.HeaderApiKey)]
        public static async Task<IActionResult> JwtAuthTest5(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }

        /// <summary>
        /// Authorization with QueryApiKey
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("JwtAuthTest6")]
        [HttpAuthorize(Scheme.QueryApiKey)]
        public static async Task<IActionResult> JwtAuthTest6(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpToken]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }
    }
}

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
        [FunctionName("AuthTest")]
        [HttpJwtAuthorize(ClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", 
            ClaimValue = "auth0|5c55ba2b77c9d67a41774e27")]
        public static async Task<IActionResult> AuthTest(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [HttpJwt]HttpUser user,
            ILogger log)
        {
            return new OkObjectResult("ok");
        }
    }
}

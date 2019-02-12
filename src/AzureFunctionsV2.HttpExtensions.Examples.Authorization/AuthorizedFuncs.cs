using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Authorization;
using AzureFunctionsV2.HttpExtensions.Examples.Authorization.Startup;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Examples.Authorization
{
    public static class AuthorizedFuncs
    {
        [FunctionName("BasicAuthenticatedFunc")]
        [HttpAuthorize(Scheme.Basic)]
        public static async Task<IActionResult> BasicAuthenticatedFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult($"Welcome anonymous, you are authorized!");
        }

        [FunctionName("ApiKeyAuthenticatedFunc")]
        [HttpAuthorize(Scheme.HeaderApiKey)]
        public static async Task<IActionResult> ApiKeyAuthenticatedFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult($"Welcome anonymous, you are authorized!");
        }

        [FunctionName("JwtAuthenticatedFunc")]
        [HttpAuthorize(Scheme.Jwt)]
        public static async Task<IActionResult> JwtAuthenticatedFunc(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [HttpToken]HttpUser user)
        {
            return new OkObjectResult($"Welcome {user.ClaimsPrincipal.Identity.Name}, you are authorized!");
        }

        [FunctionName("JwtLoginRedirect")]
        public static async Task<IActionResult> JwtLoginRedirect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "login")] HttpRequest req,
            ILogger log)
        {
            var nonce = new Random().Next();
            return new ContentResult()
            {
                Content =
                    $"<html><body><a href=\"https://jusas-tests.eu.auth0.com/authorize?response_type=id_token&scope=profile%20email%20name%20nickname&client_id=XLjNBiBCx3_CZUAK3gagLSC_PPQjBDzB&redirect_uri=http://localhost:7071/api/callback&nonce={nonce}\">Login</a></body></html>",
                ContentType = "text/html",
                StatusCode = 200
            };
        }


        [FunctionName("JwtLoginCallback")]
        public static async Task<IActionResult> JwtLoginCallback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "callback")] HttpRequest req,
            ILogger log)
        {
            return new ContentResult()
            {
                Content =
                    "<html><head><script>window.onload = function() { document.getElementById('t').innerHTML = 'Your access token is: ' + document.location.hash.substr(10); }</script></head><body><div id='t'></div></body></html>",
                ContentType = "text/html",
                StatusCode = 200
            };
        }

    }
}

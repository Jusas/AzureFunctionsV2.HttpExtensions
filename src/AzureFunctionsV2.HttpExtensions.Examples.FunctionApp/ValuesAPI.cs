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

namespace AzureFunctionsV2.HttpExtensions.Examples.FunctionApp
{
    public static class ValuesAPI
    {
        public static List<int> _values = new List<int>() {1, 2, 56, 33, 12};

        /// <summary>
        /// A simple Function, with nothing HttpExtensions specific except that
        /// the exception filter applies; try getting a value out of bounds, you'll
        /// see that the exception filter returns an error message.
        ///
        /// It is worth noting that the route parameters (index) are not processed in any way;
        /// they are the only parameters here that do not have a BindingAttribute applied
        /// to them. This is the default Functions behavior.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="index"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetValue")]
        public static async Task<IActionResult> GetValue(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "values/{index}")] HttpRequest req,
            int index,
            ILogger log)
        {
            log.LogInformation(nameof(GetValue));
            return new OkObjectResult(_values[index]);
        }

        /// <summary>
        /// Nothing of significance here, just here to complete the API.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="index"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetValues")]
        public static async Task<IActionResult> GetValues(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "values")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation(nameof(GetValues));
            return new OkObjectResult(_values);
        }

        /// <summary>
        /// Basic usage of a body param. Expecting an integer in the PUT body, and
        /// it is required. Send it with Content-Type: application/json.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("PutValue")]
        public static async Task<IActionResult> PutValue(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "values/{index}")] HttpRequest req,
            int index,
            [HttpBody(Required = true)]HttpParam<int> value,
            ILogger log)
        {
            log.LogInformation(nameof(PutValue));
            // Note the implicit operator. The actual value is in value.Value.
            _values[index] = value;
            return new OkObjectResult(_values[index]);
        }

        /// <summary>
        /// Again, for the sake of completeness.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="index"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("DeleteValue")]
        public static async Task<IActionResult> DeleteValue(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "values/{index}")] HttpRequest req,
            int index,
            ILogger log)
        {
            log.LogInformation(nameof(DeleteValue));
            // Note the implicit operator. The actual value is in value.Value.
            var value = _values[index];
            _values.RemoveAt(index);
            return new OkObjectResult(value);
        }
    }
}

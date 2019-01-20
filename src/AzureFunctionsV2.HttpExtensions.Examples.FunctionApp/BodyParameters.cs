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
    public static class BodyParameters
    {
        public class MyObject
        {
            public string Name { get; set; }
            public bool Boolean { get; set; }
            public int[] Numbers { get; set; }
        }

        public class MyObjectRequired
        {
            [JsonRequired]
            public string Name { get; set; }
            public bool Boolean { get; set; }
            public int[] Numbers { get; set; }
        }

        /*
        POST http://localhost:7071/api/post-object
        Content-Type: application/json

        {
          "name": "John",
          "boolean": true,
          "numbers": [1,2,3]
        }

        ###

        POST http://localhost:7071/api/post-object
        Content-Type: application/xml

        <MyObject>
           <Boolean>true</Boolean>
           <Name>John</Name>
           <Numbers>
              <int>1</int>
              <int>2</int>
              <int>3</int>
           </Numbers>
        </MyObject>
        */
        /// <summary>
        /// Standard HTTP POST with a body that needs to be deserialized into an object.
        /// The default deserialization to objects supports these content types:
        /// - application/json
        /// - application/xml
        /// </summary>
        /// <param name="req"></param>
        /// <param name="bodyData"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("BodyParametersDemo1")]
        public static async Task<IActionResult> PostObject(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "post-object")] HttpRequest req,
            [HttpBody]HttpParam<MyObject> bodyData,            
            ILogger log)
        {
            log.LogInformation($"Object received: {JsonConvert.SerializeObject(bodyData.Value)}");
            return new OkObjectResult("see the log");
        }

        /// <summary>
        /// Basically the same as above, but with some requirements:
        /// - Body is not allowed to be empty (Required=true)
        /// - The MyObjectRequired class' Name property has a JsonRequired attribute.
        ///   Since we use JSON deserializer, an exception will be thrown if the required
        ///   field is not set, and the method will return 400.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="bodyData"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("BodyParametersDemo2")]
        public static async Task<IActionResult> PostObject2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "post-object-required")] HttpRequest req,
            [HttpBody(Required = true)]HttpParam<MyObjectRequired> bodyData,
            ILogger log)
        {
            log.LogInformation($"Object received: {JsonConvert.SerializeObject(bodyData.Value)}");
            return new OkObjectResult("see the log");
        }



    }
}

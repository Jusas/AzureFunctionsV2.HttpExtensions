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
    public static class FormParameters
    {
        public enum SomeEnum
        {
            One,
            Ten,
            All
        }

        public class SomeClass
        {
            public string Name { get; set; }
            public bool Bool { get; set; }
        }

        /*
        POST http://localhost:7071/api/form-basics
        Content-Type: application/x-www-form-urlencoded

        someString=hello&someObject=%7B%22Name%22%3A%22John%22%2C%22Bool%22%3A%22true%22%7D&someInteger=123&stringList=5&stringList=6&stringList=7&enumArray=0&enumArray=1

        */
        /// <summary>
        /// The very basics of using the HttpFormAttribute and showing how it works
        /// deserialization-wise.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="someString"></param>
        /// <param name="someObject"></param>
        /// <param name="integer"></param>
        /// <param name="stringList"></param>
        /// <param name="enumArray"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("FormParametersDemo1")]
        public static async Task<IActionResult> FormParametersDemo1(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "form-basics")] HttpRequest req,
            [HttpForm]HttpParam<string> someString,
            [HttpForm]HttpParam<SomeClass> someObject,
            [HttpForm(Required = true, Name = "someInteger")]HttpParam<int> integer,
            [HttpForm]HttpParam<List<string>> stringList,
            [HttpForm]HttpParam<SomeEnum[]> enumArray,
            ILogger log)
        {
            log.LogInformation($"someString: {someString}");
            log.LogInformation($"someObject: {JsonConvert.SerializeObject(someObject.Value)}");
            log.LogInformation($"integer: {integer}");
            log.LogInformation($"stringList: {JsonConvert.SerializeObject(stringList.Value)}");
            log.LogInformation($"enumArray: {JsonConvert.SerializeObject(enumArray.Value)}");
            return new OkObjectResult("see the log");
        }

        /*
        POST http://localhost:7071/api/form-upload
        Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

        ------WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="someString"

        hello

        ------WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="image"; filename="test.jpg"
        Content-Type: image/jpeg
        MIME-Version: 1.0

        < c:/temp/temp.jpg
        ------WebKitFormBoundary7MA4YWxkTrZu0gW--

        */
        /// <summary>
        /// An example of a file upload. A typical multipart/form-data scenario.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="someString"></param>
        /// <param name="file"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("FormParametersDemo2")]
        public static async Task<IActionResult> FormParametersDemo2(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "form-upload")] HttpRequest req,
            [HttpForm]HttpParam<string> someString,
            [HttpForm(Name = "image")]HttpParam<IFormFile> file,
            ILogger log)
        {
            log.LogInformation($"someString: {someString}");
            log.LogInformation($"File information: name: {file.Value?.Name}, fileName: {file.Value?.FileName}, size: {file.Value?.Length}");
            return new OkObjectResult("see the log");
        }

        /*
        POST http://localhost:7071/api/form-upload-multi
        Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

        ------WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="someString"

        hello

        ------WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="image1"; filename="test.jpg"
        Content-Type: image/jpeg
        MIME-Version: 1.0

        < c:/temp/temp.jpg
        ------WebKitFormBoundary7MA4YWxkTrZu0gW
        Content-Disposition: form-data; name="image2"; filename="test.jpg"
        Content-Type: image/jpeg
        MIME-Version: 1.0

        < c:/temp/temp.jpg
        ------WebKitFormBoundary7MA4YWxkTrZu0gW--
        */
        /// <summary>
        /// Multi-file upload when there's a varying number of files.
        /// This is not really different from accessing req.Form.Files, except that it's in the
        /// Function signature, allowing some code analysis with reflection.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="someString"></param>
        /// <param name="files"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("FormParametersDemo3")]
        public static async Task<IActionResult> FormParametersDemo3(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "form-upload-multi")] HttpRequest req,
            [HttpForm]HttpParam<string> someString,
            [HttpForm]HttpParam<IFormFileCollection> files,
            ILogger log)
        {
            log.LogInformation($"someString: {someString}");
            foreach (var file in files.Value)
            {
                log.LogInformation($"File information: name: {file.Name}, fileName: {file.FileName}, size: {file.Length}");
            }
            
            return new OkObjectResult("see the log");
        }
    }
}

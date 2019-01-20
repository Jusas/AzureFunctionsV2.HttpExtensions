using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureFunctionsV2.HttpExtensions.Examples.FunctionApp
{
    public static class UserAPI
    {
        public enum UserRole
        {
            Sales,
            Manufacturing,
            ItSupport
        }

        public class NewUser
        {
            [JsonRequired]
            public string Name { get; set; }
            public string Id { get; set; }
            [JsonRequired]
            public int Age { get; set; }
            [JsonRequired]
            public bool IsAdmin { get; set; }
            [JsonRequired]
            public UserRole Role { get; set; }
        }

        public class User : NewUser
        {
            [JsonIgnore]
            public string ImageFormat { get; set; }
            [JsonIgnore]
            public string ImageBase64 { get; set; }
        }

        private static List<User> _users = new List<User>();

        /// <summary>
        /// Nothing special here, retrieves a user.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetUser")]
        public static async Task<IActionResult> GetUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation(nameof(GetUser));
            return new OkObjectResult(_users.FirstOrDefault(x => x.Id == id));
        }

        /// <summary>
        /// Returns a list of users, with optional query params:
        /// "onlyWithImage" query parameter which sets whether
        /// you only want users that have uploaded their image.
        /// "onlyRoles" is an array of enum values that sets
        /// which user roles to include in the results.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetUsers")]
        public static async Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequest req,
            [HttpQuery]HttpParam<bool> onlyWithImage,
            [HttpQuery]HttpParam<UserRole[]> onlyRoles,
            ILogger log)
        {
            // For global conversions;
            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            //{
            //    Converters = new List<JsonConverter>() { new StringEnumConverter(true) }
            //};

            log.LogInformation(nameof(GetUser));
            var result = onlyWithImage.Value ? _users.Where(x => !string.IsNullOrEmpty(x.ImageBase64)) : _users;
            if (onlyRoles.Value != null && onlyRoles.Value.Length > 0)
                result = result.Where(x => onlyRoles.Value.Contains(x.Role));
            return new OkObjectResult(result);
        }

        /// <summary>
        /// A HTTP POST that takes a User object as a form field value and an image file
        /// via file upload. This is a multipart/form-data request.
        /// The file gets assigned to "image". Since no Name parameter was given to the attribute,
        /// the code expects to find the file basically with req.Form.Files.First(f => f.Name == "image").
        /// </summary>
        /// <param name="req"></param>
        /// <param name="user"></param>
        /// <param name="image"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("PostUser")]
        public static async Task<IActionResult> PostUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequest req,
            [HttpForm(Required = true)]HttpParam<User> user,
            [HttpForm]HttpParam<IFormFile> image,
            ILogger log)
        {
            log.LogInformation(nameof(PostUser));
            var userObject = new User()
            {
                Age = user.Value.Age,
                Id = Guid.NewGuid().ToString(),
                IsAdmin = user.Value.IsAdmin,
                Name = user.Value.Name
            };
            if (image.Value != null)
            {
                using (var sr = new BinaryReader(image.Value.OpenReadStream()))
                {
                    var imageBytes = sr.ReadBytes((int)image.Value.Length);
                    userObject.ImageFormat = image.Value.ContentType;
                    userObject.ImageBase64 = Convert.ToBase64String(imageBytes);
                }
            }
            

            _users.Add(userObject);

            return new OkObjectResult(userObject);
        }

        /// <summary>
        /// Retrieves user image, using a header to determine which format to return it in.
        /// If the x-output-format header is provided, it will be used to either
        /// output the data as base64 string or served as image file.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("GetUserImage")]
        public static async Task<IActionResult> GetUserImage(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}/image")] HttpRequest req,
            string id,
            [HttpHeader(Name = "x-output-format")]HttpParam<string> outputFormat,
            ILogger log)
        {
            log.LogInformation(nameof(GetUserImage));

            var user = _users.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(outputFormat.Value) && outputFormat.Value == "base64")
            {
                return new OkObjectResult(user.ImageBase64);
            }
            
            var imageBytes = Convert.FromBase64String(user.ImageBase64);
            var imageType = user.ImageFormat;

            return new FileContentResult(imageBytes, imageType);
        }


    }
}

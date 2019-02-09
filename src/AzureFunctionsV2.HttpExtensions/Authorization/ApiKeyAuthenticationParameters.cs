using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class ApiKeyAuthenticationParameters
    {
        public string QueryParameterName { get; set; }
        public string HeaderName { get; set; }
        public Func<string, HttpRequest, Task<bool>> ApiKeyVerifier { get; set; }
    }
}

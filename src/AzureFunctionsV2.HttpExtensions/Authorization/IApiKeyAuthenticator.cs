using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public interface IApiKeyAuthenticator
    {
        Task<bool> Authenticate(string apiKey, HttpRequest request);
    }
}

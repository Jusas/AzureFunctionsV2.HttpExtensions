using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Interface for the API Key Authenticator.
    /// </summary>
    public interface IApiKeyAuthenticator
    {
        /// <summary>
        /// The authentication method.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>False if authentication failed/unauthorized, true if authentication succeeded.</returns>
        Task<bool> Authenticate(HttpRequest request);
    }
}

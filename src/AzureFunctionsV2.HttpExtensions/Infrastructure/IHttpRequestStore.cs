using System;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// A service that stores HttpRequests from HttpTriggers for cross-code access.
    /// The exception filter for example does not get the HttpRequest when it's invoked, and needs
    /// to use this service in order to gain access to it.
    /// Get/Set/Remove is performed based on the function invocation id.
    /// </summary>
    public interface IHttpRequestStore
    {
        void Set(Guid functionInvocationId, HttpRequest httpRequest);
        HttpRequest Get(Guid functionInvocationId);
        void Remove(Guid functionInvocationId);
    }
}

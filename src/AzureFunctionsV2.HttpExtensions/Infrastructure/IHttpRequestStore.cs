using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public interface IHttpRequestStore
    {
        void Set(Guid functionInvocationId, HttpRequest httpRequest);
        HttpRequest Get(Guid functionInvocationId);
        void Remove(Guid functionInvocationId);
    }
}

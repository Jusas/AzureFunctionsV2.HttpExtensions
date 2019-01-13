using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class HttpRequestStore : IHttpRequestStore
    {
        private ConcurrentDictionary<Guid, HttpRequest> _httpRequests = new ConcurrentDictionary<Guid, HttpRequest>();

        public void Set(Guid functionInvocationId, HttpRequest httpRequest)
        {
            if (_httpRequests.ContainsKey(functionInvocationId))
                return;
            _httpRequests.AddOrUpdate(functionInvocationId, httpRequest, (guid, request) => request);
        }

        public void Remove(Guid functionInvocationId)
        {
            if (_httpRequests.ContainsKey(functionInvocationId))
            {
                _httpRequests.TryRemove(functionInvocationId, out _);
            }
        }

        public HttpRequest Get(Guid functionInvocationId)
        {
            HttpRequest r = null;
            if (_httpRequests.ContainsKey(functionInvocationId))
            {
                _httpRequests.TryGetValue(functionInvocationId, out r);
            }
            return r;
        }
    }
}

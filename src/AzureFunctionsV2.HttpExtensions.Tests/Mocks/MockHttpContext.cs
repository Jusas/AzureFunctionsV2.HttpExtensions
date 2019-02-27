using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace AzureFunctionsV2.HttpExtensions.Tests.Mocks
{
    public class MockHttpContext : HttpContext
    {


        public override void Abort()
        {
            throw new NotImplementedException();
        }

        public override IFeatureCollection Features { get; }
        private HttpRequest _request;
        public override HttpRequest Request => _request;
        public void SetRequest(HttpRequest r) => _request = r;
        private HttpResponse _response;
        public override HttpResponse Response => _response;
        public void SetResponse(HttpResponse r) => _response = r;
        public override ConnectionInfo Connection { get; }
        public override WebSocketManager WebSockets { get; }
        public override AuthenticationManager Authentication { get; }
        public override ClaimsPrincipal User { get; set; }
        public override IDictionary<object, object> Items { get; set; }
        public override IServiceProvider RequestServices { get; set; }
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; }
        public override ISession Session { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Tests.Mocks
{
    public class MockHttpResponse : HttpResponse
    {

        public MockHttpResponse(MockHttpContext httpContext)
        {
            HttpContext = httpContext;
            httpContext?.SetResponse(this);
            Body = new MemoryStream();
            HeaderDictionary = new HeaderDictionary();
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override int StatusCode { get; set; }
        public override IHeaderDictionary Headers => HeaderDictionary;
        public IHeaderDictionary HeaderDictionary;
        public override Stream Body { get; set; }
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; }
    }
}

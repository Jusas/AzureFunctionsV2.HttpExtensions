using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class HttpErrorResponse
    {
        public object SerializableResponseObject { get; set; }
        public ContentType ContentType { get; set; }
    }
}

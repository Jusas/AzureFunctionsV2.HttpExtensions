using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public class HttpHeaderAttribute : Attribute
    {
        public HttpHeaderAttribute()
        {

        }

        public HttpHeaderAttribute(string headerName)
        {
            Name = headerName;
        }

        public string Name { get; set; }
        public bool Required { get; set; }
    }
}

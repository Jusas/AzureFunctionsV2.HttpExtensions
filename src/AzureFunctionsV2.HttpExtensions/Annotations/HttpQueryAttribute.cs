using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public class HttpQueryAttribute : Attribute
    {
        public HttpQueryAttribute()
        {

        }

        public HttpQueryAttribute(string queryParameterName)
        {
            Name = queryParameterName;
        }

        public string Name { get; set; }
        public bool Required { get; set; }
    }
}

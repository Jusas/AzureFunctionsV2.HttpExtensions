using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public class HttpFormAttribute : Attribute
    {
        public HttpFormAttribute()
        {

        }

        public HttpFormAttribute(string formFieldName)
        {
            Name = formFieldName;
        }

        public string Name { get; set; }
        public bool Required { get; set; }
    }
}

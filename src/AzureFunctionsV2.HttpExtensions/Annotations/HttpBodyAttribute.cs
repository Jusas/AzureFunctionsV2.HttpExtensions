using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public class HttpBodyAttribute : Attribute
    {
        public bool Required { get; set; }
    }
}

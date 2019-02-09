using System;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    /// <summary>
    /// Indicates that the value is populated from the JSON Web Token or oAuth2 token. Only valid for
    /// <see cref="HttpUser"/> type method parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class HttpTokenAttribute : Attribute
    {
    }
}

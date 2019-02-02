using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class HttpJwtAttribute : Attribute
    {
    }
}

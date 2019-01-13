using System;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    interface IHttpParam
    {
        Attribute HttpExtensionAttribute { get; set; }
    }
}

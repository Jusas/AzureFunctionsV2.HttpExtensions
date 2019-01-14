using System;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public interface IHttpParam
    {
        Attribute HttpExtensionAttribute { get; set; }
    }
}

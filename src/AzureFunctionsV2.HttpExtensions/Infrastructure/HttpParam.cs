using System;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class HttpParam<T> : IHttpParam
    {
        public Attribute HttpExtensionAttribute { get; set; }
        public T Value { get; set; }

        public static implicit operator T(HttpParam<T> param)
        {
            return param.Value;
        }
    }
}

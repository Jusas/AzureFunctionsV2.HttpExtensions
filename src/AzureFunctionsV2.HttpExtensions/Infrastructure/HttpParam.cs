using System;
using AzureFunctionsV2.HttpExtensions.Annotations;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// <para>
    /// The class that defines a Function parameter whose value is coming from a HTTP request.
    /// Together with implementations of <see cref="HttpSourceAttribute"/> they define parameters
    /// whose values can be retrieved from the trigger's <see cref="HttpRequest"/>.
    /// </para>
    /// <para>
    /// Should always be used together with the attributes.
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpParam<T> : IHttpParam
    {
        /// <summary>
        /// Reference to the <see cref="HttpSourceAttribute"/> implementation that this parameter
        /// has.
        /// </summary>
        public Attribute HttpExtensionAttribute { get; set; }

        /// <summary>
        /// The value retrieved and deserialized from the HTTP request.
        /// </summary>
        public T Value { get; set; }

        public static implicit operator T(HttpParam<T> param)
        {
            return param.Value;
        }
    }
}

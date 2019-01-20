using System;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// Interface for <see cref="HttpParam{T}"/> without generics.
    /// </summary>
    public interface IHttpParam
    {
        /// <summary>
        /// The <seealso cref="HttpExtensionAttribute"/> that has been assigned
        /// to this HttpParam.
        /// </summary>
        Attribute HttpExtensionAttribute { get; set; }
    }
}

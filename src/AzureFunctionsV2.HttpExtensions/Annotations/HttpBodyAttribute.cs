using System;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Description;

namespace AzureFunctionsV2.HttpExtensions.Annotations
{
    /// <summary>
    /// Binding attribute that indicates that the parameter value should
    /// be read from <see cref="HttpRequest.Body"/>. Apply to <see cref="HttpParam{T}"/>
    /// type parameters in the Azure Function signature.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class HttpBodyAttribute : HttpSourceAttribute
    {
        /// <summary>
        /// Indicates that this is a required field. Required fields are checked upon assignment
        /// and <see cref="ParameterRequiredException"/> will be thrown if the required field is
        /// not present or is null/empty.
        /// </summary>
        public bool Required { get; set; }
    }
}

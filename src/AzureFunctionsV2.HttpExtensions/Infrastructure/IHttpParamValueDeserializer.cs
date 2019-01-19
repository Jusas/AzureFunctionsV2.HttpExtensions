using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// A custom deserializer interface that allows fully customized deserializing of HTTP parameters.
    /// The implementation of this interface must be registered to Services at startup.
    /// </summary>
    public interface IHttpParamValueDeserializer
    {
        /// <summary>
        /// This method should deserialize the body from Stream to the target type.
        /// </summary>
        /// <param name="body">The request body</param>
        /// <param name="httpParamValueType">The target value type</param>
        /// <param name="functionName">The Function name whose parameters are being assigned</param>
        /// <param name="request">The source HttpRequest</param>
        /// <returns>The deserialization operation result</returns>
        Task<DeserializerResult> DeserializeBodyParameter(Stream body, Type httpParamValueType, string functionName, 
            HttpRequest request);

        /// <summary>
        /// This method should deserialize a header value to the target type.
        /// </summary>
        /// <param name="headerName">The header name</param>
        /// <param name="headerValue">The header source value</param>
        /// <param name="httpParamValueType">The target value type</param>
        /// <param name="functionName">The Function name whose parameters are being assigned</param>
        /// <param name="request">The source HttpRequest</param>
        /// <returns>The deserialization operation result</returns>
        Task<DeserializerResult> DeserializeHeaderParameter(string headerName, StringValues headerValue, Type httpParamValueType, 
            string functionName, HttpRequest request);

        /// <summary>
        /// This method should deserialize a form parameter value to the target type.
        /// </summary>
        /// <param name="formParameterName">The form parameter name</param>
        /// <param name="formParameterValue">The form parameter source value</param>
        /// <param name="httpParamValueType">The target value type</param>
        /// <param name="functionName">The Function name whose parameters are being assigned</param>
        /// <param name="request">The source HttpRequest</param>
        /// <returns>The deserialization operation result</returns>
        Task<DeserializerResult> DeserializeFormParameter(string formParameterName, StringValues formParameterValue, Type httpParamValueType,
            string functionName, HttpRequest request);

        /// <summary>
        /// This method should deserialize a query parameter value to the target type.
        /// </summary>
        /// <param name="queryParameterName">The query parameter name</param>
        /// <param name="queryParameterValue">The query parameter source value</param>
        /// <param name="httpParamValueType">The target value type</param>
        /// <param name="functionName">The Function name whose parameters are being assigned</param>
        /// <param name="request">The source HttpRequest</param>
        /// <returns>The deserialization operation result</returns>
        Task<DeserializerResult> DeserializeQueryParameter(string queryParameterName, StringValues queryParameterValue, Type httpParamValueType,
            string functionName, HttpRequest request);

        /// <summary>
        /// This method should deserialize a form file parameter value to the target type.
        /// </summary>
        /// <param name="fileParameterName">The form file parameter name</param>
        /// <param name="formFile">The IFormFile object that contains the data</param>
        /// <param name="httpParamValueType">The target value type</param>
        /// <param name="functionName">The Function name whose parameters are being assigned</param>
        /// <param name="request">The source HttpRequest</param>
        /// <returns>The deserialization operation result</returns>
        Task<DeserializerResult> DeserializeFormFile(string fileParameterName, IFormFile formFile,
            Type httpParamValueType, string functionName, HttpRequest request);
    }

    public class DeserializerResult
    {
        public static DeserializerResult DidNotDeserialize => new DeserializerResult(false);

        public DeserializerResult(bool didDeserialize, object result)
        {
            DidDeserialize = didDeserialize;
            Result = result;
        }

        public DeserializerResult(bool didDeserialize)
        {
            DidDeserialize = didDeserialize;
        }

        /// <summary>
        /// Set to true if this deserializer did or wants to handle deserialization,
        /// false if we should still proceed with default serialization after this deserializer was run.
        /// </summary>
        public bool DidDeserialize { get; }

        /// <summary>
        /// The resulting deserialized object.
        /// </summary>
        public object Result { get; }
    }
}

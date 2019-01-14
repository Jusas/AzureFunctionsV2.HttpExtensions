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
        /// <returns>A tuple of boolean (True if did handle deserialization, false if proceeding with default serialization)
        /// and deserialized result object values</returns>
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
        /// <returns>A tuple of boolean (True if did handle deserialization, false if proceeding with default serialization)
        /// and deserialized result object values</returns>
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
        /// <returns>A tuple of boolean (True if did handle deserialization, false if proceeding with default serialization)
        /// and deserialized result object values</returns>
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
        /// <returns>A tuple of boolean (True if did handle deserialization, false if proceeding with default serialization)
        /// and deserialized result object values</returns>
        Task<DeserializerResult> DeserializeQueryParameter(string queryParameterName, StringValues queryParameterValue, Type httpParamValueType,
            string functionName, HttpRequest request);
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

        public bool DidDeserialize { get; }
        public object Result { get; }
    }
}

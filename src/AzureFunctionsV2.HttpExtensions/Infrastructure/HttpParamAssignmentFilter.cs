using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// The <see cref="IFunctionInvocationFilter"/> implementation which runs deserialization and assigns
    /// the <see cref="HttpParam{T}"/> values from the source <see cref="HttpRequest"/> using the
    /// <see cref="FunctionExecutingContext.Arguments"/> to gain access to the HttpRequest and the HttpParams.
    /// </summary>
    public class HttpParamAssignmentFilter : IFunctionFilter, IFunctionInvocationFilter
    {
        private readonly IHttpRequestStore _httpRequestStore;
        private readonly IHttpParamValueDeserializer _paramValueDeserializer;

        public HttpParamAssignmentFilter(IHttpRequestStore httpRequestStore, IHttpParamValueDeserializer paramValueDeserializer)
        {
            _httpRequestStore = httpRequestStore;
            _paramValueDeserializer = paramValueDeserializer;
        }

        /// <summary>
        /// This is where most of the magic happens. This runs just before the Function gets executed.
        /// At this stage we only have empty value holders (HttpParams), but we get access to the
        /// HttpRequest from the context Arguments, as well as the access to all the HttpParams.
        /// This enables us to grab the values from the HttpRequest, run any deserialization we may
        /// want, and assign the values to the HttpParams.
        /// </summary>
        /// <param name="executingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            try
            {
                if (executingContext.Arguments.Values.Where(x => x != null).FirstOrDefault(
                        x => typeof(HttpRequest).IsAssignableFrom(x.GetType())) is HttpRequest httpRequest)
                {
                    _httpRequestStore.Set(executingContext.FunctionInstanceId, httpRequest);
                    foreach (var executingContextArgument in executingContext.Arguments)
                    {
                        var parameterName = executingContextArgument.Key;
                        if (executingContextArgument.Value is IHttpParam parameterValue)
                        {
                            if (parameterValue.HttpExtensionAttribute != null)
                            {
                                var attributeType = parameterValue.HttpExtensionAttribute.GetType();

                                if (attributeType == typeof(HttpBodyAttribute))
                                {
                                    bool wasAssigned = await TryAssignFromBody(httpRequest.Body, parameterValue, httpRequest, executingContext);
                                    if (!wasAssigned && ((HttpBodyAttribute)parameterValue.HttpExtensionAttribute).Required)
                                        throw new ParameterRequiredException("Body parameter is required", null, parameterName, httpRequest.HttpContext);
                                }
                                else if (attributeType == typeof(HttpFormAttribute))
                                {
                                    var formFieldName = ((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Name ??
                                                         parameterName;
                                    if (httpRequest.Form.ContainsKey(formFieldName))
                                    {
                                        if (((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Required &&
                                            string.IsNullOrEmpty(httpRequest.Form[formFieldName].ToString()))
                                        {
                                            throw new ParameterRequiredException($"Form field '{formFieldName}' is required",
                                                null, parameterName, httpRequest.HttpContext);
                                        }
                                        await TryAssignFromStringValues(httpRequest.Form[formFieldName], parameterValue,
                                            parameterName, httpRequest, executingContext);
                                    }
                                    else if (httpRequest.Form.Files.Any(x => x.Name == formFieldName))
                                    {
                                        await TryAssignFormFile(httpRequest.Form.Files.First(x => x.Name == formFieldName),
                                            parameterValue, parameterName, httpRequest, executingContext);
                                    }
                                    // TODO: make more extensible, use case: uploading multiple files that need to be deserialized into objects.
                                    else if (httpRequest.Form.Files != null &&
                                             typeof(IFormFileCollection) == GetHttpParamValueType(parameterValue))
                                    {
                                        if (!httpRequest.Form.Files.Any() &&
                                            ((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Required)
                                        {
                                            throw new ParameterRequiredException($"Form files are required", null, parameterName, httpRequest.HttpContext);
                                        }

                                        AssignFormFileCollection(httpRequest.Form.Files, parameterValue);
                                    }
                                    else if (((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Required)
                                        throw new ParameterRequiredException($"Form field '{formFieldName}' is required", null, parameterName, httpRequest.HttpContext);
                                }
                                else if (attributeType == typeof(HttpHeaderAttribute))
                                {
                                    var headerName = ((HttpHeaderAttribute)parameterValue.HttpExtensionAttribute).Name ??
                                                     parameterName;
                                    if (httpRequest.Headers.ContainsKey(headerName))
                                        await TryAssignFromStringValues(httpRequest.Headers[headerName], parameterValue, parameterName, httpRequest, executingContext);
                                    else if (((HttpHeaderAttribute)parameterValue.HttpExtensionAttribute).Required)
                                        throw new ParameterRequiredException($"Header '{headerName}' is required", null, parameterName, httpRequest.HttpContext);
                                }
                                else if (attributeType == typeof(HttpQueryAttribute))
                                {
                                    var queryParamName = ((HttpQueryAttribute)parameterValue.HttpExtensionAttribute).Name ??
                                                         parameterName;
                                    if (httpRequest.Query.ContainsKey(queryParamName))
                                        await TryAssignFromStringValues(httpRequest.Query[queryParamName], parameterValue, parameterName, httpRequest, executingContext);
                                    else if (((HttpQueryAttribute)parameterValue.HttpExtensionAttribute).Required)
                                        throw new ParameterRequiredException($"Query parameter '{queryParamName}' is required", null, parameterName, httpRequest.HttpContext);
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                var httpRequest = executingContext.Arguments.Values.FirstOrDefault(
                        x => typeof(HttpRequest).IsAssignableFrom(x.GetType()))
                    as HttpRequest;
                if (httpRequest.HttpContext.Items == null)
                    httpRequest.HttpContext.Items = new Dictionary<object, object>();
                httpRequest.HttpContext.Items.Add(nameof(HttpParamAssignmentFilter), e);
                throw;
            }
        }

        private void AssignFormFileCollection(IFormFileCollection formFileCollection, IHttpParam param)
        {
            var httpParamType = param.GetType();
            httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, formFileCollection);
        }

        private async Task TryAssignFormFile(IFormFile formFile, IHttpParam param, string parameterName, HttpRequest httpRequest,
            FunctionExecutingContext functionExecutingContext)
        {
            var httpParamType = param.GetType();
            var httpParamValueType = GetHttpParamValueType(param);

            try
            {
                if (_paramValueDeserializer != null)
                {
                    var resultFromDeserializer = await _paramValueDeserializer.DeserializeFormFile(parameterName, 
                        formFile, httpParamValueType, functionExecutingContext.FunctionName, httpRequest);
                    if (resultFromDeserializer.DidDeserialize)
                    {
                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, resultFromDeserializer.Result);
                        return;
                    }
                }

                // IFormFiles are supported and directly assigned.
                if(httpParamValueType == typeof(IFormFile))
                    httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, formFile);
                // Streams are supported.
                else if (httpParamValueType == typeof(Stream))
                    httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, formFile.OpenReadStream());
                else
                    throw new ParameterFormatConversionException("Only Stream and IFormFile targets are supported by the generic HttpParam", 
                        null, parameterName, httpRequest.HttpContext);

            }
            catch (Exception e)
            {
                throw new ParameterFormatConversionException($"Form file assignment failed", 
                    e, parameterName, httpRequest.HttpContext);
            }
        }

        private Type GetHttpParamValueType(IHttpParam param)
        {
            var httpParamType = param.GetType();
            var httpParamValueType = httpParamType.GetGenericArguments().First();
            return httpParamValueType;
        }

        private async Task<bool> TryAssignFromBody(Stream body, IHttpParam param, HttpRequest httpRequest, 
            FunctionExecutingContext functionExecutingContext)
        {
            var httpParamType = param.GetType();
            var httpParamValueType = GetHttpParamValueType(param);
            var contentType = httpRequest.ContentType;

            try
            {
                // Allow overriding from a custom deserializer.
                if (_paramValueDeserializer != null)
                {
                    var resultFromDeserializer = await _paramValueDeserializer.DeserializeBodyParameter(body, httpParamValueType, 
                        functionExecutingContext.FunctionName, httpRequest);
                    if (resultFromDeserializer.DidDeserialize)
                    {
                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, resultFromDeserializer.Result);
                        return true;
                    }
                }

                // If no custom deserialization, proceed with the supported content deserialization.

                // Support simple XML to POCO deserialization.
                if (contentType.ToLowerInvariant().EndsWith("/xml", StringComparison.OrdinalIgnoreCase))
                {
                    if (body == null)
                        return false;

                    if (body.Length == 0)
                        return false;

                    XmlSerializer xmlSerializer = new XmlSerializer(httpParamValueType);
                    var deserializedValue = xmlSerializer.Deserialize(body);
                    httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, deserializedValue);
                }
                // Support JSON to POCO (or any type) deserialization.
                else if (contentType.ToLowerInvariant().EndsWith("/json", StringComparison.OrdinalIgnoreCase))
                {
                    if (body == null)
                        return false;

                    using (StreamReader sr = new StreamReader(body))
                    {
                        var content = await sr.ReadToEndAsync();
                        if (content.Length == 0)
                            return false;

                        var deserializedValue = JsonConvert.DeserializeObject(content, httpParamValueType);
                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, deserializedValue);
                    }
                }
                // Support plain text.
                else if (contentType.ToLowerInvariant().EndsWith("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    if (body == null)
                        return false;

                    using (StreamReader sr = new StreamReader(body))
                    {
                        var content = await sr.ReadToEndAsync();
                        if (content.Length == 0)
                            return false;

                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, content);
                    }
                }
                else
                {
                    throw new ParameterFormatConversionException($"Body with content type '{contentType}' is unsupported", null, "body", httpRequest.HttpContext);
                }
                
            }
            catch (Exception e)
            {
                throw new ParameterFormatConversionException($"Failed to convert body to type '{httpParamValueType.Name}' " +
                    $"with content type '{contentType}'", e, "body", httpRequest.HttpContext);
            }

            return true;
        }

        private async Task TryAssignFromStringValues(StringValues value, IHttpParam param, string parameterName, HttpRequest httpRequest,
            FunctionExecutingContext functionExecutingContext)
        {
            var httpParamType = param.GetType();
            var httpParamValueType = httpParamType.GetGenericArguments().First();

            // Use custom deserializer if present.
            if (_paramValueDeserializer != null)
            {
                DeserializerResult resultFromDeserializer = null;
                object handlerCreatedObject = null;

                if (param.HttpExtensionAttribute is HttpFormAttribute)
                {
                    resultFromDeserializer = await _paramValueDeserializer.DeserializeFormParameter(parameterName, value,
                        httpParamValueType, functionExecutingContext.FunctionName, httpRequest);
                }
                else if (param.HttpExtensionAttribute is HttpHeaderAttribute)
                {
                    resultFromDeserializer = await _paramValueDeserializer.DeserializeHeaderParameter(parameterName, value,
                        httpParamValueType, functionExecutingContext.FunctionName, httpRequest);
                }
                else if (param.HttpExtensionAttribute is HttpQueryAttribute)
                {
                    resultFromDeserializer = await _paramValueDeserializer.DeserializeQueryParameter(parameterName, value,
                        httpParamValueType, functionExecutingContext.FunctionName, httpRequest);
                }

                if (resultFromDeserializer.DidDeserialize)
                {
                    httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, resultFromDeserializer.Result);
                    return;
                }
            }

            // Support for deserializing lists/arrays into enumerables. If StringValues contains multiple values, and the
            // HttpParam type happens to be an enumberable like a List<T> or an array, we try to deserialize it to such.
            if (value.Count > 0 && typeof(IEnumerable).IsAssignableFrom(httpParamValueType) && httpParamValueType != typeof(string))
            {
                TryAssignFromMultiStringValues(value, param, parameterName, httpRequest);
            }
            // If target HttpParam type is not enumerable or is a string, we'll just try to deserialize/assign it.
            else if (!typeof(IEnumerable).IsAssignableFrom(httpParamValueType) || httpParamValueType == typeof(string))
            {
                try
                {
                    var convertedValue = httpParamValueType == typeof(string) 
                        ? value.ToString() 
                        : JsonConvert.DeserializeObject(value, httpParamValueType);
                    httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, convertedValue);
                }
                catch (Exception e)
                {
                    throw new ParameterFormatConversionException($"Failed to assign parameter '{parameterName}' value", e, parameterName, httpRequest.HttpContext);
                }
                
            }
        }

        /// <summary>
        /// Tries to convert a StringValues object into an array or List.
        /// </summary>
        /// <param name="stringValues"></param>
        /// <param name="param"></param>
        /// <param name="parameterName"></param>
        /// <param name="httpRequest"></param>
        private void TryAssignFromMultiStringValues(StringValues stringValues, IHttpParam param, string parameterName, HttpRequest httpRequest)
        {
            try
            {
                var httpParamType = param.GetType();
                var httpParamValueType = httpParamType.GetGenericArguments().First();

                if (!typeof(IEnumerable).IsAssignableFrom(httpParamValueType))
                    throw new ParameterFormatConversionException($"Expected an iterable in argument '{parameterName}', " +
                        $"cannot convert value to target type '{httpParamValueType.Name}'", null, parameterName, httpRequest.HttpContext);

                if (httpParamValueType.IsArray || typeof(IList).IsAssignableFrom(httpParamValueType))
                {
                    var itemType = httpParamValueType.GetElementType();
                    if (itemType == null && httpParamValueType.IsGenericType)
                    {
                        itemType = httpParamValueType.GetGenericArguments().First();
                    }

                    var convertedValues = Array.ConvertAll(stringValues.ToArray(),
                        input => itemType == typeof(string) ? input : JsonConvert.DeserializeObject(input, itemType));
                    var typedValueArray = Array.CreateInstance(itemType, convertedValues.Length);
                    var listConstructor = httpParamValueType.GetConstructors()
                        .FirstOrDefault(x => x.GetParameters().Length == 1 && (x.GetParameters().First().ParameterType).IsAssignableFrom(typedValueArray.GetType()));

                    for (int i = 0; i < convertedValues.Length; i++)
                        typedValueArray.SetValue(convertedValues[i], i);

                    if (httpParamValueType.IsArray)
                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, typedValueArray);
                    else if (listConstructor != null)
                    {
                        var newList = Activator.CreateInstance(httpParamValueType, typedValueArray);
                        httpParamType.GetProperty(nameof(HttpParam<object>.Value))?.SetValue(param, newList);
                    }
                    else
                    {
                        throw new ParameterFormatConversionException($"Failed to convert input type of argument '{parameterName}' to type '{httpParamValueType}', " +
                                                                     $"no suitable constructor found", null, parameterName, httpRequest.HttpContext);
                    }
                }
                else
                {
                    throw new ParameterFormatConversionException($"Input type for argument '{parameterName}' is unsupported", null, parameterName, httpRequest.HttpContext);
                }
            }
            catch (Exception e)
            {
                throw new ParameterFormatConversionException($"Format conversion failed for argument '{parameterName}'", e, parameterName, httpRequest.HttpContext);
            }
            
        }

        /// <summary>
        /// Once the Function has been executed, we'll clean up the HttpRequest from our request store.
        /// Do not remove it in case the Function run failed; this gets executed before the exception
        /// filter and it still needs it for its own purposes.
        /// </summary>
        /// <param name="executedContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            if(executedContext.FunctionResult.Succeeded)
                _httpRequestStore.Remove(executedContext.FunctionInstanceId);
        }
    }
}

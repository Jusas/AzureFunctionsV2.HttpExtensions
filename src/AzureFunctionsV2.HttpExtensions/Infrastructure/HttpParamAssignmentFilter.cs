using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Annotations;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public class HttpParamAssignmentFilter : IFunctionFilter, IFunctionInvocationFilter
    {
        private IHttpRequestStore _httpRequestStore;

        public HttpParamAssignmentFilter(IHttpRequestStore httpRequestStore)
        {
            _httpRequestStore = httpRequestStore;
        }

        public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
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
                            // todo: what about files?
                            var attributeType = parameterValue.HttpExtensionAttribute.GetType();

                            if (attributeType == typeof(HttpBodyAttribute))
                            {
                                bool wasAssigned = TryAssignFromBody(httpRequest.Body, parameterValue, parameterName, httpRequest);
                                if(!wasAssigned && ((HttpBodyAttribute)parameterValue.HttpExtensionAttribute).Required)
                                    throw new ParameterRequiredException("Body parameter is required", null, parameterName, httpRequest.HttpContext);
                            }
                            else if(attributeType == typeof(HttpFormAttribute))
                            {
                                var formFieldName = ((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Name ??
                                                     parameterName;
                                if (httpRequest.Form.ContainsKey(formFieldName))
                                    TryAssignFromStringValues(httpRequest.Form[formFieldName], parameterValue, parameterName, httpRequest);
                                else if(((HttpFormAttribute)parameterValue.HttpExtensionAttribute).Required)
                                    throw new ParameterRequiredException($"Form field '{formFieldName}' is required", null, parameterName, httpRequest.HttpContext);
                            }
                            else if (attributeType == typeof(HttpHeaderAttribute))
                            {
                                var headerName = ((HttpHeaderAttribute) parameterValue.HttpExtensionAttribute).Name ??
                                                 parameterName;
                                if (httpRequest.Headers.ContainsKey(headerName))
                                    TryAssignFromStringValues(httpRequest.Headers[headerName], parameterValue, parameterName, httpRequest);
                                else if (((HttpHeaderAttribute)parameterValue.HttpExtensionAttribute).Required)
                                    throw new ParameterRequiredException($"Header '{headerName}' is required", null, parameterName, httpRequest.HttpContext);
                            }
                            else if (attributeType == typeof(HttpQueryAttribute))
                            {
                                var queryParamName = ((HttpQueryAttribute) parameterValue.HttpExtensionAttribute).Name ??
                                                     parameterName;
                                if (httpRequest.Query.ContainsKey(queryParamName))
                                    TryAssignFromStringValues(httpRequest.Query[queryParamName], parameterValue, parameterName, httpRequest);
                                else if (((HttpQueryAttribute)parameterValue.HttpExtensionAttribute).Required)
                                    throw new ParameterRequiredException($"Query parameter '{queryParamName}' is required", null, parameterName, httpRequest.HttpContext);
                            }
                        }
                    }

                }
            }
        }

        private bool TryAssignFromBody(Stream body, IHttpParam param, string parameterName, HttpRequest httpRequest)
        {
            // TODO
            return true;
        }

        private void TryAssignFromStringValues(StringValues value, IHttpParam param, string parameterName, HttpRequest httpRequest)
        {
            var httpParamType = param.GetType();
            var httpParamValueType = httpParamType.GetGenericArguments().First();
            if (value.Count > 0 && typeof(IEnumerable).IsAssignableFrom(httpParamValueType) && httpParamValueType != typeof(string))
            {
                TryAssignFromMultiStringValues(value, param, parameterName, httpRequest);
            }
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
                    ConstructorInfo listConstructor;
                    if (itemType == null && httpParamValueType.IsGenericType)
                    {
                        itemType = httpParamValueType.GetGenericArguments().First();
                    }

                    var convertedValues = Array.ConvertAll(stringValues.ToArray(),
                        input => itemType == typeof(string) ? input : JsonConvert.DeserializeObject(input, itemType));
                    var typedValueArray = Array.CreateInstance(itemType, convertedValues.Length);
                    listConstructor = httpParamValueType.GetConstructors()
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


        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            _httpRequestStore.Remove(executedContext.FunctionInstanceId);
        }
    }
}

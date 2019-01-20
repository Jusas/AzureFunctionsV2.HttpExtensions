using Microsoft.Azure.WebJobs;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// The converter that transforms the temporary AttributeParameters into HttpParams.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpParamConverter<T> : IConverter<AttributedParameter, HttpParam<T>>
    {
        public HttpParam<T> Convert(AttributedParameter input)
        {
            return new HttpParam<T>
            {
                HttpExtensionAttribute = input.SourceAttribute
            };
        }
    }
}

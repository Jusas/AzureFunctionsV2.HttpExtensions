using Microsoft.Azure.WebJobs;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
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

using AzureFunctionsV2.HttpExtensions.Extensions;
using Microsoft.Azure.WebJobs;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    public static class ExtensionRegistration
    {
        public static IWebJobsBuilder AddAzureFunctionHttpExtensions(this IWebJobsBuilder builder)
        {
            builder.AddExtension<HttpAttributeExtensionsConfigProvider>();
            return builder;
        }
    }
}

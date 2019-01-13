using AzureFunctionsV2.HttpExtensions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(ExtensionsStartup), "AzureFunctionsV2HttpExtensionsStartup")]

namespace AzureFunctionsV2.HttpExtensions
{
    public class ExtensionsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddAzureFunctionHttpExtensions();
            builder.Services.AddSingleton<IFunctionFilter, HttpParamAssignmentFilter>();
            builder.Services.AddSingleton<IFunctionFilter, HttpExtensionsExceptionFilter>();
            builder.Services.AddSingleton<IHttpRequestStore, HttpRequestStore>();
        }
    }
}
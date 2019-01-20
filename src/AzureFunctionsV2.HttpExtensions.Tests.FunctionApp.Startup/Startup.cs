using AzureFunctionsV2.HttpExtensions.Infrastructure;
using AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: WebJobsStartup(typeof(Startup), "MyStartup")]

namespace AzureFunctionsV2.HttpExtensions.Tests.FunctionApp.Startup
{
    // See: https://github.com/Azure/Azure-Functions/issues/972
    // This is why the startup class is now in a separate assembly.
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // builder.Services.Replace(ServiceDescriptor.Singleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>());
            // builder.Services.AddSingleton<IHttpExceptionHandler, DefaultHttpExceptionHandler>();
            // builder.Services.AddSingleton<IHttpResponseErrorFormatter, DefaultHttpResponseErrorFormatter>();
        }
    }
}

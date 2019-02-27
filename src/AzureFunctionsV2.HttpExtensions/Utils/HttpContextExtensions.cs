using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Utils
{
    public static class HttpContextExtensions
    {
        private static readonly string ExceptionListKey = "HttpExtensionsStoredExceptions";
        private static readonly string FunctionExecutingContextKey = "HttpExtensionsFunctionExecutingContext";

        public static void StoreExceptionItem(this HttpContext context, Exception e)
        {
            if(context.Items == null)
                context.Items = new Dictionary<object, object>();

            if (!context.Items.ContainsKey(ExceptionListKey))
            {
                context.Items = new Dictionary<object, object>()
                {
                    {ExceptionListKey, new List<Exception>() {e} }
                };
            }
            else
            {
                var exceptionList = context.Items[ExceptionListKey] as List<Exception>;
                if(exceptionList == null)
                    throw new InvalidOperationException($"Expected HttpContext.Items['{ExceptionListKey}'] to be a List<Exception>, " +
                                                        $"but was a {context.Items[ExceptionListKey].GetType().Name}");
                exceptionList.Add(e);
            }
        }

        public static List<Exception> GetStoredExceptions(this HttpContext context)
        {
            if (context.Items != null && context.Items.ContainsKey(ExceptionListKey) && context.Items[ExceptionListKey] is List<Exception> el)
            {
                return el;
            }

            return new List<Exception>();
        }

        public static void StoreFunctionExecutingContext(this HttpContext context, FunctionExecutingContext functionExecutingContext)
        {
            if(context.Items == null)
                context.Items = new Dictionary<object, object>();

            if (context.Items.ContainsKey(FunctionExecutingContextKey))
                context.Items[FunctionExecutingContextKey] = functionExecutingContext;
            else
                context.Items.Add(FunctionExecutingContextKey, functionExecutingContext);
        }

        public static FunctionExecutingContext GetStoredFunctionExecutingContext(this HttpContext context)
        {
            if(context.Items != null && context.Items.ContainsKey(FunctionExecutingContextKey))
                return context.Items[FunctionExecutingContextKey] as FunctionExecutingContext;
            return null;
        }
    }
}

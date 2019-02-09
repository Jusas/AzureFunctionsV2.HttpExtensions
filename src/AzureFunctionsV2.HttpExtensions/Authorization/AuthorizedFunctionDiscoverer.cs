using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Implementation of the authorized Function discoverer.    
    /// </summary>
    public class AuthorizedFunctionDiscoverer : IAuthorizedFunctionDiscoverer
    {
        /// <summary>
        /// Finds Functions that have the <see cref="HttpAuthorizeAttribute"/> attached.
        /// Scans assemblies that have been loaded, specifically the ones that refer to this
        /// assembly, and then looks for the attribute in static class static methods.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)> GetFunctions()
        {
            // Find functions from the assemblies. Criteria:
            // - member of static class
            // - member has a parameter with HttpRequest (with HttpTrigger attribute) in its signature
            // - member has FunctionNameAttribute (optional, take the name from it if it has)
            // - member has HttpAuthorizeAttribute

            var candidateAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetReferencedAssemblies()
                        .Any(r => r.Name == Assembly.GetAssembly(this.GetType()).GetName().Name));

            var functions = new Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)>();
            foreach (var candidateAssembly in candidateAssemblies)
            {
                var asmFunctionMethodsWithAuth = candidateAssembly.ExportedTypes
                    .Where(x => x.IsAbstract && x.IsSealed && x.IsClass)
                    .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    .Where(m =>
                        m.GetParameters().Any(p =>
                            p.ParameterType == typeof(HttpRequest) &&
                            p.GetCustomAttributes().Any(a => a.GetType().Name == "HttpTriggerAttribute")
                        ) &&
                        m.GetCustomAttributes<HttpAuthorizeAttribute>().Any()
                    );
                foreach (var method in asmFunctionMethodsWithAuth)
                {
                    var methodFunctionName = method.Name;
                    var functionNameAttribute = method.GetCustomAttributes()
                        .FirstOrDefault(a => a.GetType().Name == "FunctionNameAttribute");
                    if (functionNameAttribute != null)
                    {
                        var propInfo = functionNameAttribute.GetType().GetProperty("Name");
                        methodFunctionName = propInfo.GetValue(functionNameAttribute) as string ?? method.Name;
                    }

                    var authorizeAttributes = method.GetCustomAttributes<HttpAuthorizeAttribute>().ToList();
                    functions.Add(methodFunctionName, (method, authorizeAttributes));
                }
            }

            return functions;
        }
    }
}

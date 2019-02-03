using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtAuthorizedFunctionDiscoverer : IJwtAuthorizedFunctionDiscoverer
    {
        public Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)> GetFunctions()
        {
            // Find functions from the assemblies. Criteria:
            // - member of static class
            // - member has a parameter with HttpRequest (with HttpTrigger attribute) in its signature
            // - member has FunctionNameAttribute (optional, take the name from it if it has)
            // - member has HttpAuthorizeAttribute

            var candidateAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetReferencedAssemblies()
                        .Any(r => r.Name == Assembly.GetAssembly(this.GetType()).GetName().Name));

            var functions = new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>();
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
                        m.GetCustomAttribute<HttpJwtAuthorizeAttribute>() != null
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

                    var authorizeAttribute = method.GetCustomAttribute<HttpJwtAuthorizeAttribute>();
                    functions.Add(methodFunctionName, (method, authorizeAttribute));
                }
            }

            return functions;
        }
    }
}

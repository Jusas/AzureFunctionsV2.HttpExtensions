using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using AzureFunctionsV2.HttpExtensions.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtHttpAuthorizationFilter: IFunctionFilter, IFunctionInvocationFilter
    {
        private static Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)> _functionCache;
        private static readonly object _lock = new object();
        private static IJwtAuthenticator _jwtAuthenticator;

        public JwtHttpAuthorizationFilter(IJwtAuthenticator jwtAuthenticator)
        {
            _jwtAuthenticator = jwtAuthenticator;
        }

        public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if(_jwtAuthenticator == null)
                throw new HttpAuthenticationException("JWT Authenticator has not been configured");

            var (functionMethodInfo, authorizeAttribute) = GetFunction(executingContext.FunctionName);

            if (functionMethodInfo != null)
            {
                var httpRequest = executingContext.Arguments.Values.FirstOrDefault(
                        x => typeof(HttpRequest).IsAssignableFrom(x.GetType()))
                    as HttpRequest;

                if(!httpRequest.Headers.ContainsKey("Authorization"))
                    throw new HttpAuthenticationException("Authorization header is missing");
                var authToken = httpRequest.Headers["Authorization"].ToString();
                if(!authToken.StartsWith("Bearer "))
                    throw new HttpAuthenticationException("Expected Bearer token in Authorization header");
                authToken = authToken.Substring(7);

                var (claimsPrincipal, securityToken) = await _jwtAuthenticator.Authenticate(authToken);

                if (authorizeAttribute.ClaimType != null && authorizeAttribute.ClaimValue != null)
                {
                    if (!claimsPrincipal.HasClaim(authorizeAttribute.ClaimType, authorizeAttribute.ClaimValue))
                        throw new HttpAuthorizationException("User does not have the required claim");
                }

                var jwtClaimsPrincipalFunctionArg = executingContext.Arguments.Values.FirstOrDefault(
                    x => typeof(HttpUser).IsAssignableFrom(x.GetType()))
                    as HttpUser;

                if (jwtClaimsPrincipalFunctionArg != null)
                    jwtClaimsPrincipalFunctionArg.ClaimsPrincipal = claimsPrincipal;

                //var claimsPrincipal = httpContext.User;
                //var claim = new Claim(function.Item2.ClaimType, function.Item2.ClaimValue);

                //if (claimsPrincipal != null)
                //{
                //    var hasClaim = claimsPrincipal.HasClaim(claim.Type, claim.Value);
                //    if (!hasClaim)
                //        throw new AuthenticationException("User does not have the required claim");
                //}
                //else
                //{
                //    throw new AuthenticationException("User is not authenticated");
                //}
            }
        }

        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
        }

        private async Task VerifyBearerToken(HttpRequest request)
        {
        }

        private (MethodInfo methodInfo, HttpJwtAuthorizeAttribute authorizeAttribute) GetFunction(string functionName)
        {
            if (_functionCache == null)
                InitializeCache();

            if (_functionCache.ContainsKey(functionName))
                return _functionCache[functionName];

            return (null, null);
        }

        private void InitializeCache()
        {
            lock (_lock)
            {
                // Find functions from the assemblies. Criteria:
                // - member of static class
                // - member has a parameter with HttpRequest (with HttpTrigger attribute) in its signature
                // - member has FunctionNameAttribute (optional, take the name from it if it has)
                // - member has HttpAuthorizeAttribute


                var candidateAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetReferencedAssemblies()
                        .Any(r => r.Name == Assembly.GetAssembly(this.GetType()).GetName().Name));

                var cache = new Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)>();
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
                        cache.Add(methodFunctionName, (method, authorizeAttribute));
                    }
                }

                _functionCache = cache;
            }
        }

    }
}

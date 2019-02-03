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
using Microsoft.Extensions.Options;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class JwtHttpAuthorizationFilter: IFunctionFilter, IFunctionInvocationFilter
    {
        private Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)> _functionCache;
        private static readonly object _lock = new object();
        private IJwtAuthenticator _jwtAuthenticator;
        private IJwtAuthorizedFunctionDiscoverer _jwtAuthorizedFunctionDiscoverer;
        private JwtAuthenticationOptions _options;

        public JwtHttpAuthorizationFilter(IJwtAuthenticator jwtAuthenticator,
            IJwtAuthorizedFunctionDiscoverer jwtAuthorizedFunctionDiscoverer,
            IOptions<JwtAuthenticationOptions> config)
        {
            _jwtAuthenticator = jwtAuthenticator;
            _jwtAuthorizedFunctionDiscoverer = jwtAuthorizedFunctionDiscoverer;
            _options = config?.Value;
        }

        public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            if(_jwtAuthenticator == null)
                throw new InvalidOperationException("JWT Authenticator has not been configured");

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

                if (_options?.CustomAuthorizationFilter != null)
                {
                    await _options.CustomAuthorizationFilter(claimsPrincipal, securityToken);
                }
                else
                {
                    if (authorizeAttribute.ClaimType != null && authorizeAttribute.ClaimValue != null)
                    {
                        if (!claimsPrincipal.HasClaim(authorizeAttribute.ClaimType, authorizeAttribute.ClaimValue))
                            throw new HttpAuthorizationException("User does not have the required claim");
                    }
                }
                
                var jwtClaimsPrincipalFunctionArg = executingContext.Arguments.Values.FirstOrDefault(
                    x => typeof(HttpUser).IsAssignableFrom(x.GetType()))
                    as HttpUser;

                if (jwtClaimsPrincipalFunctionArg != null)
                    jwtClaimsPrincipalFunctionArg.ClaimsPrincipal = claimsPrincipal;
            }
        }

        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
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
                _functionCache = _jwtAuthorizedFunctionDiscoverer.GetFunctions();
            }
        }

    }
}

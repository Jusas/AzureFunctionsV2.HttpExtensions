using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class HttpAuthorizationFilter : IFunctionFilter, IFunctionInvocationFilter
    {
        private readonly IJwtAuthenticator _jwtAuthenticator;
        private readonly IApiKeyAuthenticator _apiKeyAuthenticator;
        private readonly IBasicAuthenticator _basicAuthenticator;
        private readonly HttpAuthenticationOptions _options;
        private readonly IAuthorizedFunctionDiscoverer _authorizedFunctionDiscoverer;
        private readonly IOAuth2Authenticator _oAuth2Authenticator;
        private static readonly object _lock = new object();
        private Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)> _functionCache;

        public HttpAuthorizationFilter(IAuthorizedFunctionDiscoverer authorizedFunctionDiscoverer,
            IOptions<HttpAuthenticationOptions> config,
            IJwtAuthenticator jwtAuthenticator,
            IApiKeyAuthenticator apiKeyAuthenticator,
            IBasicAuthenticator basicAuthenticator,
            IOAuth2Authenticator oAuth2Authenticator
            )
        {
            _jwtAuthenticator = jwtAuthenticator;
            _apiKeyAuthenticator = apiKeyAuthenticator;
            _basicAuthenticator = basicAuthenticator;
            _oAuth2Authenticator = oAuth2Authenticator;
            _options = config?.Value;
            _authorizedFunctionDiscoverer = authorizedFunctionDiscoverer;
        }

        private (MethodInfo methodInfo, IList<HttpAuthorizeAttribute> authorizeAttribute) GetFunction(string functionName)
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
                _functionCache = _authorizedFunctionDiscoverer.GetFunctions();
            }
        }

        public async Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken)
        {
            try
            {
                var (functionMethodInfo, authorizeAttributes) = GetFunction(executingContext.FunctionName);

                if (functionMethodInfo != null)
                {
                    var httpRequest = executingContext.Arguments.Values.FirstOrDefault(
                            x => typeof(HttpRequest).IsAssignableFrom(x.GetType()))
                        as HttpRequest;

                    if (authorizeAttributes.Any(a => a.Scheme == Scheme.Jwt))
                    {
                        if (!httpRequest.Headers.ContainsKey("Authorization"))
                            throw new HttpAuthenticationException("Authorization header is missing");

                        var (claimsPrincipal, securityToken) = await _jwtAuthenticator.Authenticate(
                            httpRequest.Headers["Authorization"].ToString());

                        // Call the custom filter if one exists. It is expected to throw if authorization fails.
                        if (_options?.JwtAuthentication?.AuthorizationFilter != null)
                        {
                            await _options.JwtAuthentication.AuthorizationFilter(claimsPrincipal, securityToken, authorizeAttributes);
                        }

                        var claimsPrincipalFunctionArg = executingContext.Arguments.Values.FirstOrDefault(
                                x => typeof(HttpUser).IsAssignableFrom(x.GetType()))
                            as HttpUser;

                        if (claimsPrincipalFunctionArg != null)
                            claimsPrincipalFunctionArg.ClaimsPrincipal = claimsPrincipal;
                        return;
                    }

                    if (authorizeAttributes.Any(a => a.Scheme == Scheme.OAuth2))
                    {
                        if (!httpRequest.Headers.ContainsKey("Authorization"))
                            throw new HttpAuthenticationException("Authorization header is missing");

                        // Bearer token can contain anything; leave it to the OAuth2Authenticator to handle.
                        var userClaimsPrincipal = await _oAuth2Authenticator.Authenticate(
                            httpRequest.Headers["Authorization"].ToString(), httpRequest,
                            authorizeAttributes.Where(a => a.Scheme == Scheme.OAuth2).ToList());

                        var claimsPrincipalFunctionArg = executingContext.Arguments.Values.FirstOrDefault(
                                x => typeof(HttpUser).IsAssignableFrom(x.GetType()))
                            as HttpUser;

                        if (claimsPrincipalFunctionArg != null)
                            claimsPrincipalFunctionArg.ClaimsPrincipal = userClaimsPrincipal;
                        return;
                    }

                    if (authorizeAttributes.Any(a => a.Scheme == Scheme.HeaderApiKey || a.Scheme == Scheme.QueryApiKey))
                    {
                        bool passed = await _apiKeyAuthenticator.Authenticate(httpRequest);
                        if (!passed)
                            throw new HttpAuthenticationException("Unauthorized");
                        return;
                    }

                    if (authorizeAttributes.Any(a => a.Scheme == Scheme.Basic))
                    {
                        if (!httpRequest.Headers.ContainsKey("Authorization"))
                            throw new HttpAuthenticationException("Authorization header is missing");
                        bool passed = await _basicAuthenticator.Authenticate(httpRequest.Headers["Authorization"].ToString());
                        if (!passed)
                            throw new HttpAuthenticationException("Unauthorized");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                var httpRequest = executingContext.Arguments.Values.FirstOrDefault(
                        x => typeof(HttpRequest).IsAssignableFrom(x.GetType()))
                    as HttpRequest;
                if(httpRequest.HttpContext.Items == null)
                    httpRequest.HttpContext.Items = new Dictionary<object, object>();
                httpRequest.HttpContext.Items.Add(nameof(HttpAuthorizationFilter), e);
            }
        }

        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
        }
    }
}

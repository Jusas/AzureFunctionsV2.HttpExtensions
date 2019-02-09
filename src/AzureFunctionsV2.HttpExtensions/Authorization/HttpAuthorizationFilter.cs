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
        private IJwtAuthenticator _jwtAuthenticator;
        private IApiKeyAuthenticator _apiKeyAuthenticator;
        private IBasicAuthenticator _basicAuthenticator;
        private HttpAuthenticationOptions _options;
        private IAuthorizedFunctionDiscoverer _authorizedFunctionDiscoverer;
        private IOAuth2Authenticator _oAuth2Authenticator;
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
                    var apiKeyAttributes = authorizeAttributes.Where(a =>
                            a.Scheme == Scheme.HeaderApiKey || a.Scheme == Scheme.QueryApiKey)
                        .ToList();
                    var apiKeyQueryFieldName = _options?.ApiKeyAuthentication?.QueryParameterName;
                    var apiKeyHeaderName = _options?.ApiKeyAuthentication?.HeaderName;

                    foreach (var attribute in apiKeyAttributes)
                    {
                        var apiKey = attribute.Scheme == Scheme.HeaderApiKey
                            ? httpRequest.Headers[apiKeyHeaderName].ToString()
                            : httpRequest.Query[apiKeyQueryFieldName].ToString();
                        bool passed = await _apiKeyAuthenticator.Authenticate(apiKey, httpRequest);
                        if (!passed)
                            throw new HttpAuthenticationException("Unauthorized");
                        return;
                    }
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

        public async Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
        }
    }
}

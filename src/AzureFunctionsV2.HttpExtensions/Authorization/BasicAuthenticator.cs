using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureFunctionsV2.HttpExtensions.Exceptions;
using Microsoft.Extensions.Options;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public class BasicAuthenticator : IBasicAuthenticator
    {
        private BasicAuthenticationParameters _authenticationParameters;

        public BasicAuthenticator(IOptions<HttpAuthenticationOptions> authOptions)
        {
            _authenticationParameters = authOptions?.Value?.BasicAuthentication;
        }

        public async Task<bool> Authenticate(string authorizationHeader)
        {
            if(_authenticationParameters == null)
                throw new InvalidOperationException("BasicAuthenticationParameters have not been configured");

            if(!authorizationHeader.StartsWith("Basic "))
                throw new HttpAuthenticationException("Expected a basic auth token");

            authorizationHeader = authorizationHeader.Substring(6);

            var credentialBytes = Convert.FromBase64String(authorizationHeader);
            // Assume UTF-8.
            var credentials = Encoding.UTF8.GetString(credentialBytes);
            var usernamePassword = credentials.Split(':');
            if(usernamePassword.Length != 2)
                throw new HttpAuthenticationException("Invalid credentials format, expected base64 encoded username:password");

            return _authenticationParameters.ValidCredentials.Any(c =>
                c.Key.Equals(usernamePassword[0], StringComparison.OrdinalIgnoreCase) &&
                c.Value.Equals(usernamePassword[1], StringComparison.OrdinalIgnoreCase));
        }
    }
}
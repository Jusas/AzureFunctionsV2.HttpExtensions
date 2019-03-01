using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Interface for the Basic Authenticator.
    /// </summary>
    public interface IBasicAuthenticator
    {
        /// <summary>
        /// The authentication method.
        /// </summary>
        /// <returns>False if authentication failed/unauthorized, true if authentication succeeded.</returns>
        Task<bool> Authenticate(string authorizationHeader);
    }
}

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public interface IBasicAuthenticator
    {
        Task<bool> Authenticate(string authorizationHeader);
    }
}

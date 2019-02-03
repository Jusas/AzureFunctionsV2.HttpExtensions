using System.Collections.Generic;
using System.Reflection;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    public interface IJwtAuthorizedFunctionDiscoverer
    {
        Dictionary<string, (MethodInfo, HttpJwtAuthorizeAttribute)> GetFunctions();
    }
}

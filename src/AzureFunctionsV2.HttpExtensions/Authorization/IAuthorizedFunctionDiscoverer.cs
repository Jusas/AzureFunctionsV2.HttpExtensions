using System.Collections.Generic;
using System.Reflection;

namespace AzureFunctionsV2.HttpExtensions.Authorization
{
    /// <summary>
    /// Interface for the Function discoverer that returns a set of
    /// (<see cref="MethodInfo"/>, <see cref="HttpJwtAuthorizeAttribute"/>), ie. all
    /// the Functions that bear the <see cref="HttpJwtAuthorizeAttribute"/>.
    /// </summary>
    public interface IAuthorizedFunctionDiscoverer
    {
        Dictionary<string, (MethodInfo, IList<HttpAuthorizeAttribute>)> GetFunctions();
    }
}

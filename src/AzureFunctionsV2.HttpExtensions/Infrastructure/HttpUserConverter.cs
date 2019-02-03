using AzureFunctionsV2.HttpExtensions.Authorization;
using Microsoft.Azure.WebJobs;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// The converter that transforms the temporary AttributeParameters into HttpUser.
    /// The HttpUser.ClaimsPrincipal will be later filled in the <see cref="JwtHttpAuthorizationFilter"/>.
    /// </summary>
    public class HttpUserConverter : IConverter<AttributedParameter, HttpUser>
    {
        public HttpUser Convert(AttributedParameter input)
        {
            return new HttpUser();
        }
    }
}
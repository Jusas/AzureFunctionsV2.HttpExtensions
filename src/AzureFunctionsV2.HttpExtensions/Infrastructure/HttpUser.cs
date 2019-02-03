using System.Security.Claims;
using AzureFunctionsV2.HttpExtensions.Annotations;

namespace AzureFunctionsV2.HttpExtensions.Infrastructure
{
    /// <summary>
    /// The class that defines a User parameter coming from a JSON Web Token. It is
    /// a value holder whose primary job is to hold the ClaimsPrincipal of the user
    /// that was resolved from the JWT.
    /// <para>
    /// This class should always have the binding attribute <see cref="HttpJwtAttribute"/> applied to it
    /// in the Function signature.
    /// </para>
    /// </summary>
    public class HttpUser
    {
        /// <summary>
        /// The claims of the user, originating from the JSON Web Token.
        /// </summary>
        public ClaimsPrincipal ClaimsPrincipal { get; set; }

        /// <summary>
        /// Implicit operator for easier assignment.
        /// </summary>
        /// <param name="user"></param>
        public static implicit operator ClaimsPrincipal(HttpUser user)
        {
            return user.ClaimsPrincipal;
        }
    }
}

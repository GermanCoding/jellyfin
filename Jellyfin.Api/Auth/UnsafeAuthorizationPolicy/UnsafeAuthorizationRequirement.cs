using Microsoft.AspNetCore.Authorization;

namespace Jellyfin.Api.Auth.UnsafeAuthorizationPolicy
{
    /// <summary>
    /// The unsafe authorization requirement.
    /// </summary>
    public class UnsafeAuthorizationRequirement : IAuthorizationRequirement
    {
    }
}

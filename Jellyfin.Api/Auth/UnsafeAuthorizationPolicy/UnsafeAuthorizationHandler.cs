using System;
using System.Threading.Tasks;
using Jellyfin.Api.Auth.DefaultAuthorizationPolicy;
using Jellyfin.Api.Extensions;
using Jellyfin.Data.Entities;
using Jellyfin.Extensions;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Jellyfin.Api.Auth.UnsafeAuthorizationPolicy
{
    /// <summary>
    /// Unsafe authorization handler (IP address session based authentication).
    /// </summary>
    public class UnsafeAuthorizationHandler : AuthorizationHandler<UnsafeAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISessionManager _sessionManager;
        private readonly IUserManager _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Instance of the <see cref="IHttpContextAccessor"/> interface.</param>
        /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        public UnsafeAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            ISessionManager sessionManager,
            IUserManager userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _sessionManager = sessionManager;
            _userManager = userManager;
        }

        /// <inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UnsafeAuthorizationRequirement requirement)
        {
            // This is unsafe IP-based authentication. We check if the request ip matches any active session ip address, and if so, we grant access
            // This should only be relied on for really insensitive endpoints, as it can easily be forged

            // First, check if any actual auth was given and if so, grant access anyway, overriding any ip address requirement
            var userId = context.User.GetUserId();
            if (!userId.IsEmpty())
            {
                var user = _userManager.GetUserById(userId);
                if (user != null)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            // If no actual authentication was given, fall back to ip address
            if (_httpContextAccessor.HttpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ip = _httpContextAccessor.HttpContext?.GetNormalizedRemoteIP();
            if (ip == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            foreach (var session in _sessionManager.Sessions)
            {
                if (string.Equals(ip.ToString(), session.RemoteEndPoint, StringComparison.OrdinalIgnoreCase))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }
}

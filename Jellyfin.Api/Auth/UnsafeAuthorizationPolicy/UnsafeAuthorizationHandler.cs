using System;
using System.Threading.Tasks;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Jellyfin.Api.Auth.UnsafeAuthorizationPolicy
{
    /// <summary>
    /// Unsafe authorization handler (IP address session based authentication).
    /// </summary>
    public class UnsafeAuthorizationHandler : BaseAuthorizationHandler<UnsafeAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISessionManager _sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsafeAuthorizationHandler"/> class.
        /// </summary>
        /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
        /// <param name="networkManager">Instance of the <see cref="INetworkManager"/> interface.</param>
        /// <param name="httpContextAccessor">Instance of the <see cref="IHttpContextAccessor"/> interface.</param>
        /// <param name="sessionManager">Instance of the <see cref="ISessionManager"/> interface.</param>
        public UnsafeAuthorizationHandler(
            IUserManager userManager,
            INetworkManager networkManager,
            IHttpContextAccessor httpContextAccessor,
            ISessionManager sessionManager)
            : base(userManager, networkManager, httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _sessionManager = sessionManager;
        }

        /// <inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UnsafeAuthorizationRequirement requirement)
        {
            // This is unsafe IP-based authentication. We check if the request ip matches any active session ip address, and if so, we grant access
            // This should only be relied on for really insensitive endpoints, as it can easily be forged

            // First, check if any actual auth was given and if so, grant access anyway, overriding any ip address requirement
            var validated = ValidateClaims(context.User);
            if (validated)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // If no actual authentication was given, fall back to ip address
            if (_httpContextAccessor.HttpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ip = _httpContextAccessor.HttpContext.GetNormalizedRemoteIp();

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

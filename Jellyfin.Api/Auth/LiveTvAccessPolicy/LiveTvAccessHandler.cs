﻿using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Jellyfin.Api.Auth.LiveTvAccessPolicy;

/// <summary>
/// Authorization handler for LiveTV access.
/// </summary>
public class LiveTvAccessHandler : BaseAuthorizationHandler<LiveTvAccessRequirement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiveTvAccessHandler"/> class.
    /// </summary>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="networkManager">Instance of the <see cref="INetworkManager"/> interface.</param>
    /// <param name="httpContextAccessor">Instance of the <see cref="IHttpContextAccessor"/> interface.</param>
    public LiveTvAccessHandler(
        IUserManager userManager,
        INetworkManager networkManager,
        IHttpContextAccessor httpContextAccessor)
        : base(userManager, networkManager, httpContextAccessor)
    {
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LiveTvAccessRequirement requirement)
    {
        var validated = ValidateClaims(context.User, requireLiveTvAccessPermission: true);
        if (validated)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

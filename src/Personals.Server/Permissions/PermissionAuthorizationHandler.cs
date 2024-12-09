using Personals.Common.Constants;
using Personals.Infrastructure.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Personals.Server.Permissions;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAttribute>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAttribute requirement)
    {
        if (context.User.HasClaim(claim =>
                claim.Type == ApplicationClaimTypes.Permission && claim.Value == requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
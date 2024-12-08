using Microsoft.AspNetCore.Authorization;

namespace Expensive.Infrastructure.Permissions;

public class PermissionAttribute(string permission)
    : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string Permission { get; } = permission;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
using Personals.Infrastructure.Abstractions.Services;
using System.Security.Claims;

namespace Personals.Server.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : Guid.Empty;

    public string UserName => User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public bool IsAdmin => User?.FindFirstValue("IsAdmin") == "true";

    public List<KeyValuePair<string, string>> Claims => User?.Claims.AsEnumerable()
        .Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList() ?? [];
}
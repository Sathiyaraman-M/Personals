using Expensive.Infrastructure.Abstractions.Repositories;
using Expensive.Users.Entities;

namespace Expensive.Users.Abstractions.Repositories;

public interface IUserPermissionRepository : IRepository<AppUserPermission>
{
    Task<List<AppUserPermission>> GetAllPermissionsAsync(Guid appUserId);

    Task UpdatePermissionsAsync(Guid appUserId, IEnumerable<AppUserPermission> permissions);
}
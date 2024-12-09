using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Users.Entities;

namespace Personals.Users.Abstractions.Repositories;

public interface IUserPermissionRepository : IRepository<AppUserPermission>
{
    Task<List<AppUserPermission>> GetAllPermissionsAsync(Guid appUserId);

    Task UpdatePermissionsAsync(Guid appUserId, IEnumerable<AppUserPermission> permissions);
}
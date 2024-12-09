using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Users.Entities;
using Personals.Users.Models;

namespace Personals.Users.Abstractions.Repositories;

public interface IUserRepository : IRepository<AppUser>
{
    Task<IEnumerable<AppUser>> GetAllAsync(int page, int pageSize, string? search = null);

    Task<int> GetCountAsync(string? search = null);

    Task<AppUser> GetByIdAsync(Guid id);

    Task<AppUser> GetByLoginNameAsync(string loginName);

    Task<bool> IsEmailExistsAsync(string emailAddress, List<Guid>? excludeIds = null);
    Task<bool> IsLoginNameExistsAsync(string loginName, List<Guid>? excludeIds = null);
    Task<bool> IsPhoneNumberExistsAsync(string phoneNumber, List<Guid>? excludeIds = null);
    Task<bool> IsCodeExistsAsync(string code, List<Guid>? excludeIds = null);

    Task<Guid> CreateAsync(CreateAppUserModel model);

    Task UpdateAsync(Guid id, UpdateAppUserModel model);

    Task UpdateRefreshTokenAsync(Guid id, string refreshToken, DateTime refreshTokenExpiryTime);

    Task DeleteAsync(Guid id);
}
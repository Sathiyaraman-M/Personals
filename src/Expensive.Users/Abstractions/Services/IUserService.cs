using Expensive.Common.Contracts.Users;
using Expensive.Common.Wrappers;
using Expensive.Common.Wrappers.Abstractions;

namespace Expensive.Users.Abstractions.Services;

public interface IUserService
{
    Task<PaginatedResult<UserResponse>> GetAllAsync(int page, int pageSize, string? search = null);

    Task<IResult<UserResponse>> GetByIdAsync(Guid id);

    Task<IResult<List<string>>> GetPermissionsAsync(Guid id);

    Task<IResult<UserResponse>> CreateAsync(CreateUserRequest request);

    Task<IResult> UpdateAsync(Guid id, UpdateUserRequest request);

    Task<IResult> UpdatePermissionsAsync(Guid id, UpdateUserPermissionsRequest request);

    Task<IResult> DeleteAsync(Guid id);
}
using Personals.Common.Contracts.Users;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Users.Extensions;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Abstractions.Services;
using Personals.Users.Entities;
using Personals.Users.Repositories;

namespace Personals.Users.Services;

public class UserService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ICurrentUserService currentUserService)
    : IUserService
{
    private readonly IUserRepository
        _userRepository = unitOfWork.Repository<AppUser, IUserRepository, UserRepository>();

    private readonly IUserPermissionRepository _userPermissionRepository =
        unitOfWork.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>();

    public async Task<PaginatedResult<UserResponse>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than 0", nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
        }

        var users = (await _userRepository.GetAllAsync(page, pageSize, search))
            .Select(x => x.ToResponse()).ToList();
        var serialNo = ((page - 1) * pageSize) + 1;
        users.ForEach(x => x.SerialNo = serialNo++);
        var totalUsers = await _userRepository.GetCountAsync(search);
        return PaginatedResult<UserResponse>.Create(users, page, pageSize, totalUsers);
    }

    public async Task<IResult<UserResponse>> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        var user = await _userRepository.GetByIdAsync(id);
        return SuccessfulResult<UserResponse>.Succeed(user.ToResponse());
    }

    public async Task<IResult<List<string>>> GetPermissionsAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        _ = await _userRepository.GetByIdAsync(id);
        var permissions = await _userPermissionRepository.GetAllPermissionsAsync(id);
        return SuccessfulResult<List<string>>.Succeed(permissions.Select(permission => permission.Permission).ToList());
    }

    public async Task<IResult<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (await _userRepository.IsEmailExistsAsync(request.EmailAddress))
        {
            throw new ArgumentException("Email already exists");
        }

        if (await _userRepository.IsLoginNameExistsAsync(request.LoginName))
        {
            throw new ArgumentException("Login name already exists");
        }

        if (await _userRepository.IsPhoneNumberExistsAsync(request.PhoneNumber))
        {
            throw new ArgumentException("Phone number already exists");
        }

        if (await _userRepository.IsCodeExistsAsync(request.Code))
        {
            throw new ArgumentException("Code already exists");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Password and confirm password do not match");
        }

        var hashedPassword = passwordHasher.HashPassword(request.Password);
        var user = request.ToModel(hashedPassword, currentUserService.UserName, currentUserService.UserId);

        unitOfWork.BeginTransaction();
        var userId = await _userRepository.CreateAsync(user);
        unitOfWork.CommitChanges();

        return await GetByIdAsync(userId);
    }

    public async Task<IResult> UpdatePermissionsAsync(Guid id, UpdateUserPermissionsRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        _ = await _userRepository.GetByIdAsync(id);
        var permissions = request.Permissions.Select(permission => new AppUserPermission
        {
            AppUserId = id, Permission = permission
        }).ToList();

        unitOfWork.BeginTransaction();
        await _userPermissionRepository.UpdatePermissionsAsync(id, permissions);
        unitOfWork.CommitChanges();

        return SuccessfulResult.Succeed("User permissions updated successfully!");
    }

    public async Task<IResult> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        if (await _userRepository.IsEmailExistsAsync(request.EmailAddress, [id]))
        {
            throw new ArgumentException("Email already exists");
        }

        if (await _userRepository.IsLoginNameExistsAsync(request.LoginName, [id]))
        {
            throw new ArgumentException("Login name already exists");
        }

        if (await _userRepository.IsPhoneNumberExistsAsync(request.PhoneNumber, [id]))
        {
            throw new ArgumentException("Phone number already exists");
        }

        if (await _userRepository.IsCodeExistsAsync(request.Code, [id]))
        {
            throw new ArgumentException("Code already exists");
        }

        var user = request.ToModel(currentUserService.UserName, currentUserService.UserId);

        unitOfWork.BeginTransaction();
        await _userRepository.UpdateAsync(id, user);
        unitOfWork.CommitChanges();

        return SuccessfulResult.Succeed("User updated successfully!");
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        var count = await _userRepository.GetCountAsync();
        if (count == 1)
        {
            throw new ArgumentException("Cannot delete the only user");
        }

        unitOfWork.BeginTransaction();
        await _userRepository.DeleteAsync(id);
        unitOfWork.CommitChanges();

        return SuccessfulResult.Succeed("User deleted successfully!");
    }
}
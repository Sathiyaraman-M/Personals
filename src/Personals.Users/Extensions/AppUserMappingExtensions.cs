using Personals.Common.Contracts.Users;
using Personals.Users.Entities;
using Personals.Users.Models;

namespace Personals.Users.Extensions;

public static class AppUserMappingExtensions
{
    public static AppUser ToAppUser(this CreateAppUserModel model, DateTime createdOnDateTime)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            Code = model.Code,
            LoginName = model.LoginName,
            FullName = model.FullName,
            EmailAddress = model.EmailAddress,
            PhoneNumber = model.PhoneNumber,
            PasswordHash = model.PasswordHash,
            IsActive = model.IsActive,
            CreatedByUserName = model.CreatedByUserName,
            CreatedByUserId = model.CreatedByUserId,
            CreatedOnDate = createdOnDateTime
        };
    }

    public static AppUser ToAppUser(this UpdateAppUserModel model, DateTime lastUpdatedOnDateTime)
    {
        return new AppUser
        {
            Code = model.Code,
            LoginName = model.LoginName,
            FullName = model.FullName,
            EmailAddress = model.EmailAddress,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            LastModifiedByUserName = model.LastModifiedByUserName,
            LastModifiedByUserId = model.LastModifiedByUserId,
            LastModifiedOnDate = lastUpdatedOnDateTime
        };
    }

    public static UserResponse ToResponse(this AppUser appUser)
    {
        return new UserResponse(
            appUser.Id,
            appUser.Code,
            appUser.LoginName,
            appUser.FullName,
            appUser.EmailAddress,
            appUser.PhoneNumber,
            appUser.IsActive,
            appUser.CreatedByUserName,
            appUser.CreatedByUserId,
            appUser.CreatedOnDate,
            appUser.LastModifiedByUserName,
            appUser.LastModifiedByUserId,
            appUser.LastModifiedOnDate);
    }

    public static CreateAppUserModel ToModel(this CreateUserRequest request, string passwordHash,
        string createdByUserName, Guid createdByUserId)
    {
        return new CreateAppUserModel
        {
            Code = request.Code,
            LoginName = request.LoginName,
            FullName = request.FullName,
            EmailAddress = request.EmailAddress,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            IsActive = request.IsActive,
            CreatedByUserName = createdByUserName,
            CreatedByUserId = createdByUserId
        };
    }

    public static UpdateAppUserModel ToModel(this UpdateUserRequest request, string lastModifiedByUserName,
        Guid lastModifiedByUserId)
    {
        return new UpdateAppUserModel
        {
            Code = request.Code,
            LoginName = request.LoginName,
            FullName = request.FullName,
            EmailAddress = request.EmailAddress,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            LastModifiedByUserName = lastModifiedByUserName,
            LastModifiedByUserId = lastModifiedByUserId
        };
    }
}
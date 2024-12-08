using Expensive.Common.Contracts.Users;
using Expensive.Users.Entities;
using Expensive.Users.Models;

namespace Expensive.Users.Extensions;

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
            Address1 = model.Address1,
            Address2 = model.Address2,
            City = model.City,
            PostCode = model.PostCode,
            StateCode = model.StateCode,
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
            Address1 = model.Address1,
            Address2 = model.Address2,
            City = model.City,
            PostCode = model.PostCode,
            StateCode = model.StateCode,
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
            appUser.Address1,
            appUser.Address2,
            appUser.City,
            appUser.PostCode,
            appUser.StateCode,
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
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            PostCode = request.PostCode,
            StateCode = request.StateCode,
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
            Address1 = request.Address1,
            Address2 = request.Address2,
            City = request.City,
            PostCode = request.PostCode,
            StateCode = request.StateCode,
            EmailAddress = request.EmailAddress,
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            LastModifiedByUserName = lastModifiedByUserName,
            LastModifiedByUserId = lastModifiedByUserId
        };
    }
}
using Personals.Users.Entities;
using Personals.Tests.Base.Services;

namespace Personals.Tests.Base.Factories;

public static class AppUserFactory
{
    private static readonly StubTimeProvider TimeProvider = new();

    public static AppUser Create(Guid id,
        string code = "1",
        string loginName = "john",
        string fullName = "John Doe",
        string emailAddress = "john.doe@abc.xyz",
        string phoneNumber = "1234567890",
        string passwordHash = "password",
        bool isActive = true)
    {
        return new AppUser
        {
            Id = id,
            Code = code,
            LoginName = loginName,
            FullName = fullName,
            EmailAddress = emailAddress,
            PhoneNumber = phoneNumber,
            PasswordHash = passwordHash,
            IsActive = isActive,
            CreatedByUserName = "Test User",
            CreatedByUserId = Guid.NewGuid(),
            CreatedOnDate = TimeProvider.Now,
        };
    }
}
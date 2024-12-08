using Expensive.Tests.Base.Services;
using Expensive.Users.Entities;

namespace Expensive.Tests.Base.Factories;

public static class AppUserFactory
{
    private static readonly StubTimeProvider TimeProvider = new();

    public static AppUser Create(Guid id,
        string code = "1",
        string loginName = "john",
        string fullName = "John Doe",
        string address1 = "wall street",
        string address2 = "",
        string city = "New York",
        string postCode = "123456",
        string stateCode = "NY",
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
            Address1 = address1,
            Address2 = address2,
            City = city,
            PostCode = postCode,
            StateCode = stateCode,
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
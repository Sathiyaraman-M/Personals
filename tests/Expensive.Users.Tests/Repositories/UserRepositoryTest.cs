using Dapper;
using Expensive.Infrastructure.Exceptions;
using Expensive.Infrastructure.Services;
using Expensive.Tests.Base;
using Expensive.Tests.Base.Factories;
using Expensive.Tests.Base.Services;
using Expensive.Users.Entities;
using Expensive.Users.Models;
using Expensive.Users.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Expensive.Users.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class UserRepositoryTest(DatabaseFixture databaseFixture) : IDisposable
{
    private SqlServerDbContext DbContext => new(databaseFixture.ConnectionString);
    private static StubTimeProvider TimeProvider => new();

    private static ILogger<UserRepository> Logger => new NullLogger<UserRepository>();

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = (await userRepository.GetAllAsync(1, 10)).ToList();

        // Assert
        result.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(appUsers.Count)
            .And.BeEquivalentTo(appUsers);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchString_ShouldReturnFilteredUsers()
    {
        // Arrange
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = (await userRepository.GetAllAsync(1, 10, "bruce")).ToList();

        // Assert
        result.Should().NotBeNull()
            .And.NotBeEmpty()
            .And.HaveCount(1)
            .And.BeEquivalentTo([appUsers[0]]);
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnTotalUsersCount()
    {
        // Arrange
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.GetCountAsync();

        // Assert
        result.Should().Be(appUsers.Count);
    }

    [Fact]
    public async Task GetCountAsync_WithSearchString_ShouldReturnFilteredUsersCount()
    {
        // Arrange
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.GetCountAsync("bruce");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.GetByIdAsync(appUser.Id);

        // Assert
        result.Should().NotBeNull()
            .And.BeEquivalentTo(appUser);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => userRepository.GetByIdAsync(Guid.NewGuid()));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task GetByLoginNameAsync_WithValidLoginName_ShouldReturnUser()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.GetByLoginNameAsync("bruce");

        // Assert
        result.Should().NotBeNull()
            .And.BeEquivalentTo(appUser);
    }

    [Fact]
    public async Task GetByLoginNameAsync_WithInvalidLoginName_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "alfred", fullName: "Alfred Pennyworth");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => userRepository.GetByLoginNameAsync("alfredo"));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task IsEmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne",
                emailAddress: "bruce@wayne-enterprises.com");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsEmailExistsAsync("bruce@wayne-enterprises.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailExistsAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsEmailExistsAsync("bruce@wayne-enterprises.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEmailExistsAsync_WithExistingEmailAndExcludedIds_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne",
                emailAddress: "bruce@wayne-enterprises.com");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsEmailExistsAsync("bruce@wayne-enterprises.com", [appUser.Id]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLoginNameExistsAsync_WithExistingLoginName_ShouldReturnTrue()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsLoginNameExistsAsync("bruce");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLoginNameExistsAsync_WithNonExistingLoginName_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsLoginNameExistsAsync("alfred");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLoginNameExistsAsync_WithExistingLoginNameAndExcludedIds_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsLoginNameExistsAsync("bruce", [appUser.Id]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPhoneNumberExistsAsync_WithExistingPhoneNumber_ShouldReturnTrue()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne",
                phoneNumber: "1234567890");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsPhoneNumberExistsAsync("1234567890");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPhoneNumberExistsAsync_WithNonExistingPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsPhoneNumberExistsAsync("1234567899");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPhoneNumberExistsAsync_WithExistingPhoneNumberAndExcludedIds_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne",
                phoneNumber: "1234567890");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsPhoneNumberExistsAsync("1234567890", [appUser.Id]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCodeExistsAsync_WithExistingCode_ShouldReturnTrue()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsCodeExistsAsync("01");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCodeExistsAsync_WithNonExistingCode_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsCodeExistsAsync("02");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCodeExistsAsync_WithExistingCodeAndExcludedIds_ShouldReturnFalse()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.IsCodeExistsAsync("01", [appUser.Id]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var model = new CreateAppUserModel
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            Address1 = appUser.Address1,
            Address2 = appUser.Address2,
            City = appUser.City,
            PostCode = appUser.PostCode,
            StateCode = appUser.StateCode,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            PasswordHash = appUser.PasswordHash,
            IsActive = appUser.IsActive,
            CreatedByUserName = appUser.CreatedByUserName,
            CreatedByUserId = appUser.CreatedByUserId,
        };

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var result = await userRepository.CreateAsync(model);
        transaction.Commit();

        // Assert
        appUser.Id = result;
        var createdUser = await userRepository.GetByIdAsync(result);
        createdUser.Should().NotBeNull()
            .And.BeEquivalentTo(appUser);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_ShouldUpdateUser()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var model = new UpdateAppUserModel
        {
            Code = "01",
            LoginName = "bruce",
            FullName = "Bruce Wayne",
            Address1 = "Address3",
            Address2 = "Address4",
            City = "Gotham",
            PostCode = "123456",
            StateCode = "NY",
            EmailAddress = "bruce@wayne-enterprises.com",
            PhoneNumber = "1234567890",
            IsActive = true,
            LastModifiedByUserName = "Test User",
            LastModifiedByUserId = Guid.NewGuid(),
        };
        appUser = appUser with
        {
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
            LastModifiedOnDate = TimeProvider.Now,
        };

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        await userRepository.UpdateAsync(appUser.Id, model);
        transaction.Commit();

        // Assert
        var updatedUser = await userRepository.GetByIdAsync(appUser.Id);
        updatedUser.Should().NotBeNull()
            .And.BeEquivalentTo(appUser);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var model = new UpdateAppUserModel
        {
            Code = "01",
            LoginName = "bruce",
            FullName = "Bruce Wayne",
            Address1 = "Address3",
            Address2 = "Address4",
            City = "Gotham",
            PostCode = "123456",
            StateCode = "NY",
            EmailAddress = "bruce@wayne-enterprises.com",
            PhoneNumber = "1234567890",
            IsActive = true,
            LastModifiedByUserName = "Test User",
            LastModifiedByUserId = Guid.NewGuid(),
        };

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => userRepository.UpdateAsync(Guid.NewGuid(), model));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WithValidId_ShouldUpdateUserRefreshToken()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var refreshToken = Guid.NewGuid().ToString();
        var refreshTokenExpiration = TimeProvider.Now.AddMinutes(5);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        await userRepository.UpdateRefreshTokenAsync(appUser.Id, refreshToken, refreshTokenExpiration);
        transaction.Commit();

        // Assert
        var updatedUser = await userRepository.GetByIdAsync(appUser.Id);
        updatedUser.RefreshToken.Should().Be(refreshToken);
        updatedUser.RefreshTokenExpiryTime.Should().BeCloseTo(refreshTokenExpiration, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var refreshToken = Guid.NewGuid().ToString();
        var refreshTokenExpiration = TimeProvider.Now.AddMinutes(5);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            userRepository.UpdateRefreshTokenAsync(Guid.NewGuid(), refreshToken, refreshTokenExpiration));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        await userRepository.DeleteAsync(appUser.Id);
        transaction.Commit();

        // Assert
        var exception = await Record.ExceptionAsync(() => userRepository.GetByIdAsync(appUser.Id));
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userRepository = new UserRepository(connection, transaction, TimeProvider, Logger);

        // Act
        var exception = await Record.ExceptionAsync(() => userRepository.DeleteAsync(Guid.NewGuid()));

        // Assert
        exception.Should().NotBeNull()
            .And.BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    private async Task InsertAppUsersAsync(List<AppUser> appUsers)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var appUser in appUsers)
        {
            await connection.ExecuteAsync(""" 
                                              INSERT INTO [dbo].[AppUsers] (Id, Code, LoginName, FullName, Address1, Address2, City, PostCode, StateCode, EmailAddress, PhoneNumber, PasswordHash, IsActive, CreatedOnDate, CreatedByUserName, CreatedByUserId)
                                              VALUES (@Id, @Code, @LoginName, @FullName, @Address1, @Address2, @City, @PostCode, @StateCode, @EmailAddress, @PhoneNumber, @PasswordHash, @IsActive, @CreatedOnDate, @CreatedByUserName, @CreatedByUserId)
                                          """, appUser, transaction);
        }

        transaction.Commit();
        connection.Close();
    }

    private async Task ClearCompaniesTableAsync()
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("DELETE FROM [dbo].[AppUsers]", transaction: transaction);
        transaction.Commit();
        connection.Close();
    }

    public void Dispose()
    {
        ClearCompaniesTableAsync().Wait();
    }
}
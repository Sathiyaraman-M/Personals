using Dapper;
using Expensive.Common.Constants;
using Expensive.Infrastructure.Services;
using Expensive.Tests.Base;
using Expensive.Tests.Base.Factories;
using Expensive.Users.Entities;
using Expensive.Users.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Expensive.Users.Tests.Repositories;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class UserPermissionRepositoryTest(DatabaseFixture databaseFixture) : IDisposable
{
    private SqlServerDbContext DbContext => new(databaseFixture.ConnectionString);

    private static ILogger<UserPermissionRepository> Logger => new NullLogger<UserPermissionRepository>();

    [Fact]
    public async Task GetAllPermissionsAsync_ShouldReturnPermissionsForSpecifiedAppUser()
    {
        // Arrange
        var appUserId1 = Guid.NewGuid();
        var appUserId2 = Guid.NewGuid();
        await InsertAppUserAsync(appUserId1, "01", "user1");
        await InsertAppUserAsync(appUserId2, "02", "user2");

        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUserId1, Permission = Permissions.LookupTypes.View },
            new() { AppUserId = appUserId1, Permission = Permissions.LookupTypes.Create },
            new() { AppUserId = appUserId1, Permission = Permissions.LookupTypes.Update },
            new() { AppUserId = appUserId2, Permission = Permissions.LookupTypes.Delete }
        };
        await InsertAppUserPermissionsAsync(permissions);
        var expectedPermissions = permissions.Where(p => p.AppUserId == appUserId1).ToList();

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userPermissionRepository = new UserPermissionRepository(connection, transaction, Logger);

        // Act
        var userPermissions = await userPermissionRepository.GetAllPermissionsAsync(appUserId1);

        // Assert
        userPermissions.Should().NotBeNull().And.HaveCount(3);
        userPermissions.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public async Task GetAllPermissionsAsync_ShouldReturnEmptyListWhenNoPermissionsFound()
    {
        // Arrange
        var appUserId = Guid.NewGuid();
        await InsertAppUserAsync(appUserId, "01", "user1");

        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userPermissionRepository = new UserPermissionRepository(connection, transaction, Logger);

        // Act
        var userPermissions = await userPermissionRepository.GetAllPermissionsAsync(appUserId);

        // Assert
        userPermissions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task UpdatePermissionsAsync_ShouldUpdatePermissionsForSpecifiedAppUser()
    {
        // Arrange
        var appUserId = Guid.NewGuid();
        await InsertAppUserAsync(appUserId, "01", "user1");

        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.View },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Create },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Update }
        };
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userPermissionRepository = new UserPermissionRepository(connection, transaction, Logger);

        // Act
        await userPermissionRepository.UpdatePermissionsAsync(appUserId, permissions);
        transaction.Commit();

        // Assert
        var updatedPermissions = (await userPermissionRepository.GetAllPermissionsAsync(appUserId)).ToList();
        updatedPermissions.Should().NotBeNull().And.HaveCount(3);
        updatedPermissions.Should().BeEquivalentTo(permissions);
    }

    [Fact]
    public async Task UpdatePermissionsAsync_ShouldDeleteExistingPermissionsBeforeInsertingNewPermissions()
    {
        // Arrange
        var appUserId = Guid.NewGuid();
        await InsertAppUserAsync(appUserId, "01", "user1");

        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.View },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Create },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Update }
        };
        await InsertAppUserPermissionsAsync(permissions);

        var newPermissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Delete },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.View },
            new() { AppUserId = appUserId, Permission = Permissions.LookupTypes.Update }
        };
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        var userPermissionRepository = new UserPermissionRepository(connection, transaction, Logger);

        // Act
        await userPermissionRepository.UpdatePermissionsAsync(appUserId, newPermissions);
        transaction.Commit();

        // Assert
        var updatedPermissions = (await userPermissionRepository.GetAllPermissionsAsync(appUserId)).ToList();
        updatedPermissions.Should().NotBeNull().And.HaveCount(3);
        updatedPermissions.Should().BeEquivalentTo(newPermissions);
    }

    private async Task InsertAppUserAsync(Guid appUserId, string code, string loginName)
    {
        var appUser = AppUserFactory.Create(appUserId, code, loginName);
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync(""" 
                                          INSERT INTO [dbo].[AppUsers] (Id, Code, LoginName, FullName, Address1, Address2, City, PostCode, StateCode, EmailAddress, PhoneNumber, PasswordHash, IsActive, CreatedOnDate, CreatedByUserName, CreatedByUserId)
                                          VALUES (@Id, @Code, @LoginName, @FullName, @Address1, @Address2, @City, @PostCode, @StateCode, @EmailAddress, @PhoneNumber, @PasswordHash, @IsActive, @CreatedOnDate, @CreatedByUserName, @CreatedByUserId)
                                      """, appUser, transaction);
        transaction.Commit();
        connection.Close();
    }

    private async Task InsertAppUserPermissionsAsync(IEnumerable<AppUserPermission> permissions)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var permission in permissions)
        {
            await connection.ExecuteAsync(
                "INSERT INTO [dbo].[AppUserPermissions] ([AppUserId], [Permission]) VALUES (@AppUserId, @Permission)",
                permission,
                transaction
            );
        }

        transaction.Commit();
        connection.Close();
    }

    private async Task ClearAppUserAndAppUserPermissionsTablesAsync()
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
        ClearAppUserAndAppUserPermissionsTablesAsync().Wait();
    }
}
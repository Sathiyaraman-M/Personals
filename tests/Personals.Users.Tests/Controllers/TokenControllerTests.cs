using Dapper;
using Personals.Common.Contracts.Tokens;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Infrastructure.Services;
using Personals.Server;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using Personals.Tests.Base.Services;
using Personals.Users.Abstractions.Utilities;
using Personals.Users.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using TimeProvider = Personals.Infrastructure.Services.TimeProvider;

namespace Personals.Users.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class TokenControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _databaseFixture;

    public TokenControllerTests(WebApplicationFactory<Program> factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        Environment.SetEnvironmentVariable("JWT_SECRET", TestConstants.JwtSecret);
    }

    private SqlServerDbContext DbContext => new(_databaseFixture.ConnectionString);
    private static StubTimeProvider TimeProvider => new();

    private const string LoginName = "user";
    private const string Password = "password";

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenResponse()
    {
        // Arrange
        var customFactory = GetCustomWebApplicationFactory();
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.HashPassword(Password);
        var configuration = customFactory.Services.CreateScope().ServiceProvider
            .GetRequiredService<IJwtTokenConfiguration>();

        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName, "User", passwordHash: passwordHash);
        await InsertAppUsersAsync([appUser]);
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUserPermissionsAsync(permissions);

        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };
        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);
        response.EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeTrue();

        var tokenResponse = result.Data;
        tokenResponse.Token.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshTokenExpires.Should().BeCloseTo(
            TimeProvider.Now.AddMinutes(configuration.RefreshTokenExpirationInMinutes), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidLoginName_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var customFactory = GetCustomWebApplicationFactory();
        var tokenRequest = new TokenRequest { LoginName = "invalidUser", Password = Password };
        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ValidationFailedResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var customFactory = GetCustomWebApplicationFactory();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName, "User", passwordHash: "passwordHash");
        await InsertAppUsersAsync([appUser]);

        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = "invalidPassword" };
        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ValidationFailedResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnTokenResponse()
    {
        // Arrange
        var actualTimeProvider = new TimeProvider();
        var customFactory = GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<ITimeProvider>();
            services.AddScoped<ITimeProvider>(_ => actualTimeProvider);
        });
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.HashPassword(Password);
        var configuration = customFactory.Services.CreateScope().ServiceProvider
            .GetRequiredService<IJwtTokenConfiguration>();

        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName, "User", passwordHash: passwordHash);
        await InsertAppUsersAsync([appUser]);
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUserPermissionsAsync(permissions);

        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };
        var client = customFactory.CreateClient();
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<SuccessfulResult<TokenResponse>>();
        var tokenResponse = loginResult!.Data;

        var refreshTokenRequest = new RefreshTokenRequest
        {
            Token = tokenResponse.Token, RefreshToken = tokenResponse.RefreshToken
        };

        // Act
        response = await client.PostAsJsonAsync("api/token/refresh", refreshTokenRequest);
        response.EnsureSuccessStatusCode();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResult = await response.Content.ReadFromJsonAsync<SuccessfulResult<TokenResponse>>();
        refreshResult.Should().NotBeNull();
        refreshResult!.Succeeded.Should().BeTrue();

        tokenResponse = refreshResult.Data;
        tokenResponse.Should().NotBeNull();
        tokenResponse.Token.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshTokenExpires.Should().BeCloseTo(
            actualTimeProvider.Now.AddMinutes(configuration.RefreshTokenExpirationInMinutes), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var customFactory = GetCustomWebApplicationFactory();
        var refreshTokenRequest = new RefreshTokenRequest
        {
            Token = "invalidToken", RefreshToken = "invalidRefreshToken"
        };
        var client = customFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("api/token/refresh", refreshTokenRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ValidationFailedResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Invalid token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var actualTimeProvider = new TimeProvider();
        var customFactory = GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<ITimeProvider>();
            services.AddScoped<ITimeProvider>(_ => actualTimeProvider);
        });
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.HashPassword(Password);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName, "User", passwordHash: passwordHash);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = TimeProvider.Now.AddMinutes(1);
        await InsertAppUsersAsync([appUser]);

        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };
        var client = customFactory.CreateClient();
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<SuccessfulResult<TokenResponse>>();
        var tokenResponse = loginResult!.Data;

        var refreshTokenRequest = new RefreshTokenRequest
        {
            Token = tokenResponse.Token, RefreshToken = "invalidRefreshToken"
        };

        // Act
        response = await client.PostAsJsonAsync("api/token/refresh", refreshTokenRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ValidationFailedResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Invalid token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var actualTimeProvider = new TimeProvider();
        var customFactory = GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<ITimeProvider>();
            services.AddScoped<ITimeProvider>(_ => actualTimeProvider);
        });
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordHash = passwordHasher.HashPassword(Password);

        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName, "User", passwordHash: passwordHash);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = actualTimeProvider.Now.AddMinutes(-1);
        await InsertAppUsersAsync([appUser]);
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUserPermissionsAsync(permissions);

        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };
        var client = customFactory.CreateClient();
        var response = await client.PostAsJsonAsync("api/token/login", tokenRequest);

        var loginResult = await response.Content.ReadFromJsonAsync<SuccessfulResult<TokenResponse>>();
        var tokenResponse = loginResult!.Data;

        var refreshTokenRequest = new RefreshTokenRequest
        {
            Token = tokenResponse.Token, RefreshToken = "refreshToken"
        };

        // Act
        response = await client.PostAsJsonAsync("api/token/refresh", refreshTokenRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ValidationFailedResult<TokenResponse>>();
        result.Should().NotBeNull();
        result!.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Invalid token");
    }

    private WebApplicationFactory<Program> GetCustomWebApplicationFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        return _factory.GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<IDbContext>();
            services.AddScoped<IDbContext>(_ => DbContext);
            services.RemoveAll<ITimeProvider>();
            services.AddScoped<ITimeProvider>(_ => TimeProvider);
            configureServices?.Invoke(services);
        });
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

    private async Task InsertAppUserPermissionsAsync(List<AppUserPermission> appUserPermissions)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var appUserPermission in appUserPermissions)
        {
            await connection.ExecuteAsync(""" 
                                              INSERT INTO [dbo].[AppUserPermissions] (AppUserId, Permission)
                                              VALUES (@AppUserId, @Permission)
                                          """, appUserPermission, transaction);
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
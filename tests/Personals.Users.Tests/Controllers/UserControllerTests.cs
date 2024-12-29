using Dapper;
using Personals.Common.Constants;
using Personals.Common.Contracts.Users;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Exceptions;
using Personals.Infrastructure.Services;
using Personals.Server;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Fixtures;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Entities;
using Personals.Users.Extensions;
using Personals.Users.Models;
using Personals.Users.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute.ExceptionExtensions;
using Personals.Infrastructure.Abstractions.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace Personals.Users.Tests.Controllers;

[Collection(nameof(DatabaseCollectionFixtures))]
public sealed class UserControllerTests(
    WebApplicationFactory<Program> factory,
    DatabaseFixture databaseFixture
) : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private SqlServerDbContext DbContext => new(databaseFixture.ConnectionString);

    private static readonly string JwtBearer = TestJwtBearerBuilder
        .CreateWithDefaultClaims()
        .WithPermissions(Permissions.Users.View, Permissions.Users.ViewPermissions, Permissions.Users.Create,
            Permissions.Users.Update, Permissions.Users.UpdatePermissions, Permissions.Users.Delete)
        .Build();

    [Fact]
    public async Task GetAllUsersAsync_ReturnsPaginatedListOfUsers()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);
        var expectedResponses = GetUserResponses(appUsers, x => x.FullName);

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<UserResponse>>("/api/users");

        // Assert
        response.Should().NotBeNull();
        var data = response!.Data;
        data.Should().NotBeNull();
        data.Should().HaveCount(expectedResponses.Count);
        data.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithSearchText_ReturnsFilteredPaginatedListOfUsers()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
        };
        await InsertAppUsersAsync(appUsers);
        var expectedResponses = GetUserResponses(appUsers, x => x.FullName).Where(x => x.LoginName.Contains("bruce"))
            .ToList();

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<UserResponse>>("/api/users?searchText=bruce");

        // Assert
        response.Should().NotBeNull();
        var data = response!.Data;
        data.Should().NotBeNull();
        data.Should().HaveCount(expectedResponses.Count);
        data.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithPageAndPageSize_ReturnsPaginatedListOfUsers()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        await ClearAppUsersTableAsync();
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "03", loginName: "diana", fullName: "Diana Prince"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "04", loginName: "barry", fullName: "Barry Allen"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "05", loginName: "hal", fullName: "Hal Jordan"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "06", loginName: "arthur", fullName: "Arthur Curry"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "07", loginName: "victor", fullName: "Victor Stone"),
        };
        await InsertAppUsersAsync(appUsers);
        var expectedResponses = GetUserResponses(appUsers, x => x.FullName)
            .Skip(3).Take(3).ToList();

        // Act
        var response = await client.GetFromJsonAsync<PaginatedResult<UserResponse>>("/api/users?page=2&pageSize=3");

        // Assert
        response.Should().NotBeNull();
        var data = response!.Data;
        data.Should().NotBeNull();
        data.Should().HaveCount(expectedResponses.Count);
        data.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithPageAndPageSizeAndSearchText_ReturnsFilteredPaginatedListOfUsers()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUsers = new List<AppUser>
        {
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "03", loginName: "diana", fullName: "Diana Prince"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "04", loginName: "barry", fullName: "Barry Allen"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "05", loginName: "hal", fullName: "Hal Jordan"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "06", loginName: "arthur", fullName: "Arthur Curry"),
            AppUserFactory.Create(id: Guid.NewGuid(), code: "07", loginName: "victor", fullName: "Victor Stone"),
        };
        await InsertAppUsersAsync(appUsers);
        var expectedResponses = GetUserResponses(appUsers, x => x.FullName)
            .Where(x => x.LoginName.Contains("arthur"))
            .Skip(3).Take(3)
            .ToList();

        // Act
        var response =
            await client.GetFromJsonAsync<PaginatedResult<UserResponse>>(
                "/api/users?page=2&pageSize=3&searchText=arthur");

        // Assert
        response.Should().NotBeNull();
        var data = response!.Data;
        data.Should().NotBeNull();
        data.Should().HaveCount(expectedResponses.Count);
        data.Should().BeEquivalentTo(expectedResponses);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithInvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync("/api/users?page=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync("/api/users?pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.GetAllAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserAsync_ReturnsUser()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var expectedResponse = appUser.ToResponse();

        // Act
        var response = await client.GetFromJsonAsync<SuccessfulResult<UserResponse>>($"/api/users/{appUser.Id}");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<SuccessfulResult<UserResponse>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserAsync_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.GetByIdAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsUserPermissions()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUsersAsync([appUser]);
        await InsertAppUserPermissionsAsync(permissions);
        var expectedPermissions = permissions.Select(x => x.Permission).ToList();

        // Act
        var response =
            await client.GetFromJsonAsync<SuccessfulResult<List<string>>>($"/api/users/{appUser.Id}/permissions");

        // Assert
        var result = response.Should().NotBeNull().And.BeOfType<SuccessfulResult<List<string>>>().Subject;
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(expectedPermissions.Count);
        result.Data.Should().BeEquivalentTo(expectedPermissions);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.Empty}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());

        // Act
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            var userPermissionRepository = Substitute.For<IUserPermissionRepository>();
            userPermissionRepository.GetAllPermissionsAsync(Arg.Any<Guid>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            unitOfWork.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>()
                .Returns(userPermissionRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);
        var appUser =
            AppUserFactory.Create(id: Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        // Act
        var response = await client.GetAsync($"/api/users/{appUser.Id}/permissions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull()
            .And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsCreatedUser()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new CreateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };
        var expectedResponse = appUser.ToResponse();

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult<UserResponse>>();
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult<UserResponse>>();
        result!.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();

        var userResponse = result.Data;
        userResponse.Id.Should().NotBeEmpty();
        userResponse.Code.Should().Be(expectedResponse.Code);
        userResponse.LoginName.Should().Be(expectedResponse.LoginName);
        userResponse.FullName.Should().Be(expectedResponse.FullName);
        userResponse.EmailAddress.Should().Be(expectedResponse.EmailAddress);
        userResponse.PhoneNumber.Should().Be(expectedResponse.PhoneNumber);
        userResponse.IsActive.Should().Be(expectedResponse.IsActive);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenEmailExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser1]);
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent",
            emailAddress: appUser1.EmailAddress);
        var request = new CreateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenLoginNameExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser1]);
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", fullName: "Clark Kent",
            loginName: appUser1.LoginName);
        var request = new CreateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenPhoneNumberExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser1]);
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", fullName: "Clark Kent",
            phoneNumber: appUser1.PhoneNumber);
        var request = new CreateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenCodeExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser1]);
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: appUser1.Code, fullName: "Clark Kent");
        var request = new CreateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsBadRequest_WhenPasswordAndConfirmPasswordDoNotMatch()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new CreateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
            Password = "password",
            ConfirmPassword = "password1",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new CreateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new CreateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUserAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.IsEmailExistsAsync(Arg.Any<string>()).ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new CreateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
            Password = "password",
            ConfirmPassword = "password",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ReturnsOk_WhenPasswordChanged()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string loginName = "bruce";
        var jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(userId.ToString())
            .WithLoginName(loginName)
            .Build();
        
        var customFactory = GetCustomWebApplicationFactory();
        var client = customFactory.CreateClientWithJwtBearer(jwtBearer);
        
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        const string oldPassword = "old_password";
        var oldPasswordHash = passwordHasher.HashPassword(oldPassword);
        
        var appUser = AppUserFactory.Create(userId, code: "01", loginName: loginName, fullName: "Bruce Wayne", passwordHash: oldPasswordHash);
        await InsertAppUsersAsync([appUser]);
        
        var request = new ChangePasswordRequest { CurrentPassword = oldPassword, NewPassword = "password", ConfirmPassword = "password" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult>();
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>().Which.Succeeded.Should().BeTrue();

        var unitOfWork = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userRepository = unitOfWork.Repository<AppUser, IUserRepository, UserRepository>();
        var updatedUser = await userRepository.GetByIdAsync(appUser.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.PasswordHash.Should().NotBeNullOrEmpty();
        
        var passwordChanged = passwordHasher.VerifyHashedPassword(updatedUser.PasswordHash, request.NewPassword);
        passwordChanged.Should().BeTrue();
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string loginName = "bruce";
        var jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(userId.ToString())
            .WithLoginName(loginName)
            .Build();
        
        var customFactory = GetCustomWebApplicationFactory();
        var client = customFactory.CreateClientWithJwtBearer(jwtBearer);
        
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        const string oldPassword = "old_password";
        var oldPasswordHash = passwordHasher.HashPassword(oldPassword);
        
        var appUser = AppUserFactory.Create(userId, code: "01", loginName: loginName, fullName: "Bruce Wayne", passwordHash: oldPasswordHash);
        await InsertAppUsersAsync([appUser]);
        
        var request = new ChangePasswordRequest { CurrentPassword = "incorrect_password", NewPassword = "password", ConfirmPassword = "password" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ReturnsBadRequest_WhenNewPasswordAndConfirmPasswordDoNotMatch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string loginName = "bruce";
        var jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(userId.ToString())
            .WithLoginName(loginName)
            .Build();
        
        var customFactory = GetCustomWebApplicationFactory();
        var client = customFactory.CreateClientWithJwtBearer(jwtBearer);
        
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        const string oldPassword = "old_password";
        var oldPasswordHash = passwordHasher.HashPassword(oldPassword);
        
        var appUser = AppUserFactory.Create(userId, code: "01", loginName: loginName, fullName: "Bruce Wayne", passwordHash: oldPasswordHash);
        await InsertAppUsersAsync([appUser]);
        
        var request = new ChangePasswordRequest { CurrentPassword = oldPassword, NewPassword = "password", ConfirmPassword = "password1" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }
    
    [Fact]
    public async Task ChangePasswordAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();
        var request = new ChangePasswordRequest { CurrentPassword = "old_password", NewPassword = "password", ConfirmPassword = "password" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string loginName = "bruce";
        var jwtBearer = TestJwtBearerBuilder
            .CreateWithDefaultClaims()
            .WithUserId(userId.ToString())
            .WithLoginName(loginName)
            .Build();
        
        var customFactory = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.GetByLoginNameAsync(Arg.Any<string>()).ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        });
        var client = customFactory.CreateClientWithJwtBearer(jwtBearer);
        
        var passwordHasher = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IPasswordHasher>();
        const string oldPassword = "old_password";
        var oldPasswordHash = passwordHasher.HashPassword(oldPassword);
        
        var appUser = AppUserFactory.Create(userId, code: "01", loginName: loginName, fullName: "Bruce Wayne", passwordHash: oldPasswordHash);
        await InsertAppUsersAsync([appUser]);
        
        var request = new ChangePasswordRequest { CurrentPassword = oldPassword, NewPassword = "password", ConfirmPassword = "password" };

        // Act
        var response = await client.PostAsJsonAsync("/api/users/change-password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsOk_WhenPermissionsUpdated()
    {
        // Arrange
        var customFactory = GetCustomWebApplicationFactory();
        var client = customFactory.CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUsersAsync([appUser]);
        await InsertAppUserPermissionsAsync(permissions);
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission3", "permission4"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult>();
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>().Which.Succeeded.Should().BeTrue();

        var unitOfWork = customFactory.Services.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
        var userPermissionRepository =
            unitOfWork.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>();
        var updatedPermissions = await userPermissionRepository.GetAllPermissionsAsync(appUser.Id);
        updatedPermissions.Should().NotBeNull();
        updatedPermissions.Should().HaveCount(request.Permissions.Count);
        updatedPermissions.Select(x => x.Permission).Should().BeEquivalentTo(request.Permissions);
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission1", "permission2"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.Empty}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission1", "permission2"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission1", "permission2"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission1", "permission2"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserPermissionsAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            var userPermissionRepository = Substitute.For<IUserPermissionRepository>();
            userPermissionRepository.UpdatePermissionsAsync(Arg.Any<Guid>(), Arg.Any<List<AppUserPermission>>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            unitOfWork.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>()
                .Returns(userPermissionRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };
        await InsertAppUsersAsync([appUser]);
        await InsertAppUserPermissionsAsync(permissions);
        var request = new UpdateUserPermissionsRequest { Permissions = ["permission3", "permission4"] };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}/permissions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsUpdatedUser()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var request = new UpdateUserRequest
        {
            Code = "02",
            LoginName = "clark",
            FullName = "Clark Kent",
            EmailAddress = "clark@daily-planet.com",
            PhoneNumber = "9876543210",
            IsActive = true,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SuccessfulResult>();
        result.Should().NotBeNull().And.BeOfType<SuccessfulResult>();
        result!.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new UpdateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{Guid.Empty}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenEmailExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);
        var request = new UpdateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser1.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser2.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenLoginNameExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);
        var request = new UpdateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser1.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser2.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenPhoneNumberExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);
        var request = new UpdateUserRequest
        {
            Code = appUser2.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser1.PhoneNumber,
            IsActive = appUser2.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser2.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsBadRequest_WhenCodeExists()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);
        var request = new UpdateUserRequest
        {
            Code = appUser1.Code,
            LoginName = appUser2.LoginName,
            FullName = appUser2.FullName,
            EmailAddress = appUser2.EmailAddress,
            PhoneNumber = appUser2.PhoneNumber,
            IsActive = appUser2.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser2.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new UpdateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var request = new UpdateUserRequest
        {
            Code = appUser.Code,
            LoginName = appUser.LoginName,
            FullName = appUser.FullName,
            EmailAddress = appUser.EmailAddress,
            PhoneNumber = appUser.PhoneNumber,
            IsActive = appUser.IsActive,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdateAppUserModel>())
                .ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);
        var request = new UpdateUserRequest
        {
            Code = "02",
            LoginName = "clark",
            FullName = "Clark Kent",
            EmailAddress = "clark@daily-planet.com",
            PhoneNumber = "9876543210",
            IsActive = true,
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{appUser.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsNoContent()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);

        // Act
        var response = await client.DeleteAsync($"/api/users/{appUser1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsBadRequest_WhenIdIsEmpty()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/users/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);

        // Act
        var response = await client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsBadRequest_WhenUserIsTheOnlyUser()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClientWithJwtBearer(JwtBearer);
        await ClearAppUsersTableAsync();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        // Act
        var response = await client.DeleteAsync($"/api/users/{appUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory().CreateClient();
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        // Act
        var response = await client.DeleteAsync($"/api/users/{appUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsForbidden_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory()
            .CreateClientWithJwtBearer(TestJwtBearerBuilder.CreateWithDefaultClaims().Build());
        var appUser = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        await InsertAppUsersAsync([appUser]);

        // Act
        var response = await client.DeleteAsync($"/api/users/{appUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var client = GetCustomWebApplicationFactory(services =>
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userRepository = Substitute.For<IUserRepository>();
            userRepository.DeleteAsync(Arg.Any<Guid>()).ThrowsAsync(new DatabaseOperationFailedException());
            unitOfWork.Repository<AppUser, IUserRepository, UserRepository>().Returns(userRepository);
            services.RemoveAll<IUnitOfWork>();
            services.AddScoped<IUnitOfWork>(_ => unitOfWork);
        }).CreateClientWithJwtBearer(JwtBearer);
        var appUser1 = AppUserFactory.Create(Guid.NewGuid(), code: "01", loginName: "bruce", fullName: "Bruce Wayne");
        var appUser2 = AppUserFactory.Create(Guid.NewGuid(), code: "02", loginName: "clark", fullName: "Clark Kent");
        await InsertAppUsersAsync([appUser1, appUser2]);

        // Act
        var response = await client.DeleteAsync($"/api/users/{appUser1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var result = await response.Content.ReadFromJsonAsync<GenericFailedResult>();
        result.Should().NotBeNull().And.BeOfType<GenericFailedResult>().Which.Succeeded.Should().BeFalse();
    }

    private async Task InsertAppUsersAsync(List<AppUser> appUsers)
    {
        using var connection = DbContext.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var appUser in appUsers)
        {
            await connection.ExecuteAsync(""" 
                                              INSERT INTO [dbo].[AppUsers] (Id, Code, LoginName, FullName, EmailAddress, PhoneNumber, PasswordHash, IsActive, CreatedOnDate, CreatedByUserName, CreatedByUserId)
                                              VALUES (@Id, @Code, @LoginName, @FullName, @EmailAddress, @PhoneNumber, @PasswordHash, @IsActive, @CreatedOnDate, @CreatedByUserName, @CreatedByUserId)
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

    private WebApplicationFactory<Program> GetCustomWebApplicationFactory(
        Action<IServiceCollection>? configureServices = null)
    {
        return factory.GetCustomWebApplicationFactory(services =>
        {
            services.RemoveAll<IDbContext>();
            services.AddScoped<IDbContext>(_ => DbContext);
            configureServices?.Invoke(services);
        });
    }

    private static List<UserResponse> GetUserResponses<TKey>(IList<AppUser> appUsers,
        Func<AppUser, TKey>? orderByExpression)
    {
        var userResponses = orderByExpression == null
            ? appUsers.Select(x => x.ToResponse()).ToList()
            : appUsers.OrderBy(orderByExpression).Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        userResponses.ForEach(x => x.SerialNo = serialNo++);
        return userResponses;
    }

    private async Task ClearAppUsersTableAsync()
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
        ClearAppUsersTableAsync().Wait();
    }
}
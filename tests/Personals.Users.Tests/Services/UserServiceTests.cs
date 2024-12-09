using Personals.Common.Constants;
using Personals.Common.Contracts.Users;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Services;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Infrastructure.Exceptions;
using Personals.Tests.Base.Factories;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Entities;
using Personals.Users.Extensions;
using Personals.Users.Models;
using Personals.Users.Repositories;
using Personals.Users.Services;
using NSubstitute.ExceptionExtensions;

namespace Personals.Users.Tests.Services;

public class UserServiceTests
{
    private readonly IUnitOfWork _unitOfWorkStub = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _userRepositoryStub = Substitute.For<IUserRepository>();

    private readonly IUserPermissionRepository _userPermissionRepositoryStub =
        Substitute.For<IUserPermissionRepository>();

    private readonly IPasswordHasher _passwordHasherStub = Substitute.For<IPasswordHasher>();
    private readonly ICurrentUserService _currentUserServiceStub = Substitute.For<ICurrentUserService>();

    private const string AdminUserName = "admin";
    private static readonly Guid AdminUserId = Guid.NewGuid();

    private UserService UserService
    {
        get
        {
            _unitOfWorkStub.Repository<AppUser, IUserRepository, UserRepository>().Returns(_userRepositoryStub);
            _unitOfWorkStub.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>()
                .Returns(_userPermissionRepositoryStub);
            return new UserService(_unitOfWorkStub, _passwordHasherStub, _currentUserServiceStub);
        }
    }

    public UserServiceTests()
    {
        _currentUserServiceStub.UserName.Returns(AdminUserName);
        _currentUserServiceStub.UserId.Returns(AdminUserId);
        _currentUserServiceStub.IsAuthenticated.Returns(true);
        _currentUserServiceStub.IsAdmin.Returns(true);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<AppUser>
        {
            AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1"),
            AppUserFactory.Create(Guid.NewGuid(), code: "2", fullName: "User 2")
        };

        _userRepositoryStub.GetAllAsync(1, 10).Returns(users);
        _userRepositoryStub.GetCountAsync().Returns(users.Count);

        var expectedUsers = GetUsers(users);

        // Act
        var result = await UserService.GetAllAsync(1, 10);

        // Assert
        result.Should().BeOfType<PaginatedResult<UserResponse>>();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(expectedUsers);

        await _userRepositoryStub.Received(1).GetAllAsync(1, 10);
        await _userRepositoryStub.Received(1).GetCountAsync();
    }

    [Fact]
    public async Task GetAllAsync_WithPageLessThanOne_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.GetAllAsync(0, 10));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Page must be greater than 0 (Parameter 'page')");
    }

    [Fact]
    public async Task GetAllAsync_WithPageSizeLessThanOne_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.GetAllAsync(1, 0));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Page size must be greater than 0 (Parameter 'pageSize')");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");

        _userRepositoryStub.GetByIdAsync(user.Id).Returns(user);

        var expectedUser = user.ToResponse();

        // Act
        var result = await UserService.GetByIdAsync(user.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult<UserResponse>>();
        result.Data.Should().BeEquivalentTo(expectedUser);

        await _userRepositoryStub.Received(1).GetByIdAsync(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.GetByIdAsync(Guid.Empty));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Id cannot be empty (Parameter 'id')");
    }

    [Fact]
    public async Task GetPermissionsAsync_ShouldReturnPermissions()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = user.Id, Permission = Permissions.Users.View },
            new() { AppUserId = user.Id, Permission = Permissions.LookupTypes.View }
        };

        _userRepositoryStub.GetByIdAsync(user.Id).Returns(user);
        _userPermissionRepositoryStub.GetAllPermissionsAsync(user.Id).Returns(permissions);

        var expectedPermissions = permissions.Select(x => x.Permission).ToList();

        // Act
        var result = await UserService.GetPermissionsAsync(user.Id);

        // Assert
        result.Should().BeOfType<SuccessfulResult<List<string>>>();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(expectedPermissions);

        await _userPermissionRepositoryStub.Received(1).GetAllPermissionsAsync(user.Id);
    }

    [Fact]
    public async Task GetPermissionsAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.GetPermissionsAsync(Guid.Empty));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Id cannot be empty (Parameter 'id')");
    }

    [Fact]
    public async Task GetPermissionsAsync_WithUserNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");

        _userRepositoryStub.GetByIdAsync(user.Id).ThrowsAsync(new EntityNotFoundException("User not found"));

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.GetPermissionsAsync(user.Id));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedUser()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password",
        };

        _passwordHasherStub.HashPassword(createUserRequest.Password).Returns(user.PasswordHash);
        _userRepositoryStub.IsEmailExistsAsync(user.EmailAddress).Returns(false);
        _userRepositoryStub.IsLoginNameExistsAsync(user.LoginName).Returns(false);
        _userRepositoryStub.IsPhoneNumberExistsAsync(user.PhoneNumber).Returns(false);
        _userRepositoryStub.IsCodeExistsAsync(user.Code).Returns(false);
        _userRepositoryStub.CreateAsync(Arg.Any<CreateAppUserModel>()).Returns(user.Id);
        _userRepositoryStub.GetByIdAsync(user.Id).Returns(user);

        var expectedUser = user.ToResponse();

        // Act
        var result = await UserService.CreateAsync(createUserRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<UserResponse>>();
        result.Data.Should().BeEquivalentTo(expectedUser);
        await _userRepositoryStub.Received(1).CreateAsync(Arg.Any<CreateAppUserModel>());
    }

    [Fact]
    public async Task CreateAsync_WithExistingEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password"
        };

        _userRepositoryStub.IsEmailExistsAsync(user.EmailAddress).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.CreateAsync(createUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Email already exists");
    }

    [Fact]
    public async Task CreateAsync_WithExistingLoginName_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password"
        };

        _userRepositoryStub.IsLoginNameExistsAsync(user.LoginName).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.CreateAsync(createUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Login name already exists");
    }

    [Fact]
    public async Task CreateAsync_WithExistingPhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password"
        };

        _userRepositoryStub.IsPhoneNumberExistsAsync(user.PhoneNumber).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.CreateAsync(createUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Phone number already exists");
    }

    [Fact]
    public async Task CreateAsync_WithExistingCode_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password"
        };

        _userRepositoryStub.IsCodeExistsAsync(user.Code).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.CreateAsync(createUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Code already exists");
    }

    [Fact]
    public async Task CreateAsync_WithPasswordMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var createUserRequest = new CreateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
            Password = "password",
            ConfirmPassword = "password1"
        };

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.CreateAsync(createUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Password and confirm password do not match");
    }

    [Fact]
    public async Task UpdatePermissionsAsync_ShouldUpdateUserPermissions()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var permissions = new List<string>
        {
            Permissions.Users.View, Permissions.Users.Create, Permissions.LookupTypes.View
        };
        var updateUserPermissionsRequest = new UpdateUserPermissionsRequest { Permissions = permissions };

        _userRepositoryStub.GetByIdAsync(user.Id).Returns(user);

        // Act
        var result = await UserService.UpdatePermissionsAsync(user.Id, updateUserPermissionsRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult>()
            .Which.Messages.Should().Contain("User permissions updated successfully!");

        await _userPermissionRepositoryStub.Received(1)
            .UpdatePermissionsAsync(user.Id, Arg.Any<List<AppUserPermission>>());
    }

    [Fact]
    public async Task UpdatePermissionsAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() =>
            UserService.UpdatePermissionsAsync(Guid.Empty, new UpdateUserPermissionsRequest()));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Id cannot be empty (Parameter 'id')");
    }

    [Fact]
    public async Task UpdatePermissionsAsync_WithUserNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var permissions = new List<string>
        {
            Permissions.Users.View, Permissions.Users.Create, Permissions.LookupTypes.View
        };
        var updateUserPermissionsRequest = new UpdateUserPermissionsRequest { Permissions = permissions };

        _userRepositoryStub.GetByIdAsync(user.Id).ThrowsAsync(new EntityNotFoundException("User not found"));

        // Act
        var exception = await Record.ExceptionAsync(() =>
            UserService.UpdatePermissionsAsync(user.Id, updateUserPermissionsRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<EntityNotFoundException>()
            .Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedUser()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var updateUserRequest = new UpdateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
        };


        // Act
        var result = await UserService.UpdateAsync(user.Id, updateUserRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult>()
            .Which.Messages.Should().Contain("User updated successfully!");

        await _userRepositoryStub.Received(1).UpdateAsync(user.Id, Arg.Any<UpdateAppUserModel>());
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.UpdateAsync(Guid.Empty, new UpdateUserRequest()));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Id cannot be empty (Parameter 'id')");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var updateUserRequest = new UpdateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber,
        };

        _userRepositoryStub.IsEmailExistsAsync(Arg.Any<string>(), Arg.Any<List<Guid>>()).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.UpdateAsync(user.Id, updateUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Email already exists");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingLoginName_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var updateUserRequest = new UpdateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber
        };

        _userRepositoryStub.IsLoginNameExistsAsync(Arg.Any<string>(), Arg.Any<List<Guid>>()).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.UpdateAsync(user.Id, updateUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Login name already exists");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingPhoneNumber_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var updateUserRequest = new UpdateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber
        };

        _userRepositoryStub.IsPhoneNumberExistsAsync(Arg.Any<string>(), Arg.Any<List<Guid>>()).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.UpdateAsync(user.Id, updateUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Phone number already exists");
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCode_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");
        var updateUserRequest = new UpdateUserRequest
        {
            Code = user.Code,
            LoginName = user.LoginName,
            FullName = user.FullName,
            EmailAddress = user.EmailAddress,
            Address1 = user.Address1,
            Address2 = user.Address2,
            City = user.City,
            PostCode = user.PostCode,
            StateCode = user.StateCode,
            IsActive = user.IsActive,
            PhoneNumber = user.PhoneNumber
        };

        _userRepositoryStub.IsCodeExistsAsync(Arg.Any<string>(), Arg.Any<List<Guid>>()).Returns(true);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.UpdateAsync(user.Id, updateUserRequest));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Code already exists");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");

        _userRepositoryStub.GetCountAsync().Returns(2);

        // Act
        await UserService.DeleteAsync(user.Id);

        // Assert
        await _userRepositoryStub.Received(1).DeleteAsync(user.Id);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.DeleteAsync(Guid.Empty));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Id cannot be empty (Parameter 'id')");
    }

    [Fact]
    public async Task DeleteAsync_WithSingleUser_ShouldThrowArgumentException()
    {
        // Arrange
        var user = AppUserFactory.Create(Guid.NewGuid(), code: "1", fullName: "User 1");

        _userRepositoryStub.GetCountAsync().Returns(1);

        // Act
        var exception = await Record.ExceptionAsync(() => UserService.DeleteAsync(user.Id));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Be("Cannot delete the only user");
    }

    private static List<UserResponse> GetUsers(List<AppUser> users)
    {
        var userResponses = users.Select(x => x.ToResponse()).ToList();
        var serialNo = 1;
        userResponses.ForEach(x => x.SerialNo = serialNo++);
        return userResponses;
    }
}
using Personals.Common.Constants;
using Personals.Common.Contracts.Tokens;
using Personals.Common.Wrappers;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Infrastructure.Exceptions;
using Personals.Tests.Base;
using Personals.Tests.Base.Factories;
using Personals.Tests.Base.Services;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Abstractions.Utilities;
using Personals.Users.Entities;
using Personals.Users.Repositories;
using Personals.Users.Services;
using Personals.Users.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;

namespace Personals.Users.Tests.Services;

public class TokenServiceTests
{
    private readonly IUnitOfWork _unitOfWorkStub = Substitute.For<IUnitOfWork>();
    private readonly IUserRepository _userRepositoryStub = Substitute.For<IUserRepository>();

    private readonly IUserPermissionRepository _userPermissionRepositoryStub =
        Substitute.For<IUserPermissionRepository>();

    private readonly IJwtTokenHandler _jwtTokenHandlerStub = Substitute.For<IJwtTokenHandler>();
    private readonly IJwtTokenConfiguration _jwtTokenConfigurationStub = Substitute.For<IJwtTokenConfiguration>();
    private readonly IPasswordHasher _passwordHasherStub = Substitute.For<IPasswordHasher>();
    private readonly StubTimeProvider _timeProvider = new();

    private readonly ILogger<TokenService> _logger = new NullLogger<TokenService>();

    private TokenService TokenService
    {
        get
        {
            _unitOfWorkStub.Repository<AppUser, IUserRepository, UserRepository>().Returns(_userRepositoryStub);
            _unitOfWorkStub.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>()
                .Returns(_userPermissionRepositoryStub);
            return new TokenService(_unitOfWorkStub, _jwtTokenHandlerStub, _jwtTokenConfigurationStub,
                _passwordHasherStub, _timeProvider, _logger);
        }
    }

    private const string LoginName = "admin";
    private const string Password = "password";

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenResponse()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };

        _userRepositoryStub.GetByLoginNameAsync(LoginName).Returns(appUser);
        _passwordHasherStub.VerifyHashedPassword(appUser.PasswordHash, Password).Returns(true);
        _userPermissionRepositoryStub.GetAllPermissionsAsync(appUser.Id).Returns(permissions);
        _jwtTokenHandlerStub.BuildToken(Arg.Any<List<Claim>>()).Returns("token");
        _jwtTokenConfigurationStub.RefreshTokenExpirationInMinutes.Returns(5);

        // Act
        var result = await TokenService.LoginAsync(tokenRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<TokenResponse>>();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshTokenExpires.Should().BeCloseTo(_timeProvider.Now.AddMinutes(5), TimeSpan.FromSeconds(1));

        _passwordHasherStub.Received(1).VerifyHashedPassword(appUser.PasswordHash, Password);
        await _userPermissionRepositoryStub.Received(1).GetAllPermissionsAsync(appUser.Id);
        _jwtTokenHandlerStub.Received(1).BuildToken(Arg.Any<List<Claim>>());
        await _userRepositoryStub.Received(1)
            .UpdateRefreshTokenAsync(appUser.Id, Arg.Any<string>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        var tokenRequest = new TokenRequest { LoginName = LoginName, Password = Password };

        _userRepositoryStub.GetByLoginNameAsync(LoginName).Returns(appUser);
        _passwordHasherStub.VerifyHashedPassword(appUser.PasswordHash, Password).Returns(false);

        // Act
        var result = await TokenService.LoginAsync(tokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
        result.Messages.Should().Contain("Invalid credentials");

        _passwordHasherStub.Received(1).VerifyHashedPassword(appUser.PasswordHash, Password);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidLoginName_ShouldReturnValidationFailedResult()
    {
        // Arrange
        var tokenRequest = new TokenRequest { LoginName = "invalidUser", Password = Password };

        _userRepositoryStub.GetByLoginNameAsync("invalidUser").ThrowsAsync(new EntityNotFoundException());

        // Act
        var result = await TokenService.LoginAsync(tokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
        result.Messages.Should().Contain("Invalid credentials");

        await _userRepositoryStub.Received(1).GetByLoginNameAsync("invalidUser");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnTokenResponse()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = _timeProvider.Now.AddMinutes(5);
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, appUser.Id.ToString());
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest =
            new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = appUser.RefreshToken };
        var permissions = new List<AppUserPermission>
        {
            new() { AppUserId = appUser.Id, Permission = "permission1" },
            new() { AppUserId = appUser.Id, Permission = "permission2" }
        };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);
        _userRepositoryStub.GetByIdAsync(appUser.Id).Returns(appUser);
        _userPermissionRepositoryStub.GetAllPermissionsAsync(appUser.Id).Returns(permissions);
        _jwtTokenHandlerStub.BuildToken(Arg.Any<List<Claim>>()).Returns("newToken");
        _jwtTokenConfigurationStub.RefreshTokenExpirationInMinutes.Returns(5);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<SuccessfulResult<TokenResponse>>();
        result.Data.Token.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshTokenExpires.Should()
            .BeCloseTo(_timeProvider.Now.AddMinutes(_jwtTokenConfigurationStub.RefreshTokenExpirationInMinutes),
                TimeSpan.FromSeconds(1));

        await _userRepositoryStub.Received(1).GetByIdAsync(appUser.Id);
        await _userPermissionRepositoryStub.Received(1).GetAllPermissionsAsync(appUser.Id);
        _jwtTokenHandlerStub.Received(1).BuildToken(Arg.Any<List<Claim>>());
        await _userRepositoryStub.Received(1)
            .UpdateRefreshTokenAsync(appUser.Id, Arg.Any<string>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenUserIdClaimIsNotPresent()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = _timeProvider.Now.AddMinutes(5);
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>()));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest =
            new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = appUser.RefreshToken };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenUserIdClaimIsInvalid()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = _timeProvider.Now.AddMinutes(5);
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, "invalidUserId");
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest =
            new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = appUser.RefreshToken };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenTokenIsInvalid()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest { Token = "somejwt", RefreshToken = "refreshToken" };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Throws(new SecurityTokenException());

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenTokenIsNotJwt()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequest { Token = "not-a-jwt", RefreshToken = "refreshToken" };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token)
            .Throws(new SecurityTokenArgumentException());

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, userId.ToString());
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest = new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = "refreshToken" };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);
        _userRepositoryStub.GetByIdAsync(userId).ThrowsAsync(new EntityNotFoundException());

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenRefreshTokenIsNotPresentInDatabase()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = null;
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, appUser.Id.ToString());
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest = new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = "refreshToken" };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);
        _userRepositoryStub.GetByIdAsync(appUser.Id).Returns(appUser);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = _timeProvider.Now.AddMinutes(5);
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, appUser.Id.ToString());
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest =
            new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = "invalidRefreshToken" };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);
        _userRepositoryStub.GetByIdAsync(appUser.Id).Returns(appUser);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>();
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnValidationFailedResult_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var appUser = AppUserFactory.Create(Guid.NewGuid(), "01", LoginName);
        appUser.RefreshToken = "refreshToken";
        appUser.RefreshTokenExpiryTime = _timeProvider.Now.AddDays(-1);
        var userIdClaim = new Claim(ApplicationClaimTypes.UserId, appUser.Id.ToString());
        var expectedClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([userIdClaim]));
        var sampleJwtToken = BuildTestJwt(expectedClaimsPrincipal.Claims);
        var refreshTokenRequest =
            new RefreshTokenRequest { Token = sampleJwtToken, RefreshToken = appUser.RefreshToken };

        _jwtTokenHandlerStub.ExtractClaimsPrincipal(refreshTokenRequest.Token).Returns(expectedClaimsPrincipal);
        _userRepositoryStub.GetByIdAsync(appUser.Id).Returns(appUser);

        // Act
        var result = await TokenService.RefreshTokenAsync(refreshTokenRequest);

        // Assert
        result.Should().BeOfType<ValidationFailedResult<TokenResponse>>().Which.Messages.Should()
            .Contain("Refresh Token expired");
    }

    private string BuildTestJwt(IEnumerable<Claim> claims, DateTime? expires = null)
    {
        if (expires != null)
        {
            _timeProvider.Now.Returns(
                expires.Value.AddMinutes(0 - _jwtTokenConfigurationStub.RefreshTokenExpirationInMinutes));
        }

        _jwtTokenConfigurationStub.Secret.Returns(TestConstants.JwtSecret);

        return new JwtTokenHandler(_jwtTokenConfigurationStub, _timeProvider)
            .BuildToken(claims.ToList());
    }
}
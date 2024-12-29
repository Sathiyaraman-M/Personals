using Personals.Common.Abstractions.Services;
using Personals.Common.Constants;
using Personals.Common.Contracts.Tokens;
using Personals.Common.Wrappers;
using Personals.Common.Wrappers.Abstractions;
using Personals.Infrastructure.Abstractions.Repositories;
using Personals.Infrastructure.Abstractions.Utilities;
using Personals.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Personals.Users.Abstractions.Repositories;
using Personals.Users.Abstractions.Services;
using Personals.Users.Abstractions.Utilities;
using Personals.Users.Entities;
using Personals.Users.Repositories;
using System.Security.Claims;

namespace Personals.Users.Services;

public partial class TokenService(
    IUnitOfWork unitOfWork,
    IJwtTokenHandler jwtTokenHandler,
    IJwtTokenConfiguration jwtTokenConfiguration,
    IPasswordHasher passwordHasher,
    ITimeProvider timeProvider,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly IUserRepository
        _userRepository = unitOfWork.Repository<AppUser, IUserRepository, UserRepository>();

    private readonly IUserPermissionRepository _userPermissionRepository =
        unitOfWork.Repository<AppUserPermission, IUserPermissionRepository, UserPermissionRepository>();

    public async Task<IResult<TokenResponse>> LoginAsync(TokenRequest tokenRequest)
    {
        try
        {
            var appUser = await _userRepository.GetByLoginNameAsync(tokenRequest.LoginName);

            if (!passwordHasher.VerifyHashedPassword(appUser.PasswordHash, tokenRequest.Password))
            {
                return ValidationFailedResult<TokenResponse>.Fail("Invalid credentials");
            }

            var jwtToken = jwtTokenHandler.BuildToken(await GetClaimsAsync(appUser));
            var refreshToken = Guid.NewGuid().ToString();
            var refreshTokenExpiration =
                timeProvider.Now.AddMinutes(jwtTokenConfiguration.RefreshTokenExpirationInMinutes);

            unitOfWork.BeginTransaction();
            await _userRepository.UpdateRefreshTokenAsync(appUser.Id, refreshToken, refreshTokenExpiration);
            unitOfWork.CommitChanges();

            return SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
            {
                Token = jwtToken, RefreshToken = refreshToken, RefreshTokenExpires = refreshTokenExpiration
            });
        }
        catch (EntityNotFoundException e)
        {
            LogUserNotFound(logger, e);
            return ValidationFailedResult<TokenResponse>.Fail("Invalid credentials");
        }
    }

    public async Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        try
        {
            var claimsPrincipal = jwtTokenHandler.ExtractClaimsPrincipal(refreshTokenRequest.Token);
            var userIdString = claimsPrincipal.FindFirst(ApplicationClaimTypes.UserId)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return ValidationFailedResult<TokenResponse>.Fail("Invalid token");
            }

            var appUser = await _userRepository.GetByIdAsync(userId);
            if (appUser.RefreshToken == null && appUser.RefreshTokenExpiryTime == null)
            {
                return ValidationFailedResult<TokenResponse>.Fail("There is no login session to refresh");
            }

            if (appUser.RefreshToken != refreshTokenRequest.RefreshToken)
            {
                return ValidationFailedResult<TokenResponse>.Fail("Invalid token");
            }

            if (appUser.RefreshTokenExpiryTime.GetValueOrDefault() < timeProvider.Now)
            {
                return ValidationFailedResult<TokenResponse>.Fail("Refresh Token expired");
            }

            var newJwtToken = jwtTokenHandler.BuildToken(await GetClaimsAsync(appUser));
            var newRefreshToken = Guid.NewGuid().ToString();
            var newRefreshTokenExpiration =
                timeProvider.Now.AddMinutes(jwtTokenConfiguration.RefreshTokenExpirationInMinutes);

            unitOfWork.BeginTransaction();
            await _userRepository.UpdateRefreshTokenAsync(appUser.Id, newRefreshToken, newRefreshTokenExpiration);
            unitOfWork.CommitChanges();

            return SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
            {
                Token = newJwtToken, RefreshToken = newRefreshToken, RefreshTokenExpires = newRefreshTokenExpiration
            });
        }
        catch (SecurityTokenException e)
        {
            LogInvalidToken(logger, e);
            return ValidationFailedResult<TokenResponse>.Fail(e.Message);
        }
        catch (SecurityTokenArgumentException e)
        {
            LogInvalidToken(logger, e);
            return ValidationFailedResult<TokenResponse>.Fail("Invalid token");
        }
        catch (EntityNotFoundException e)
        {
            LogUserNotFound(logger, e);
            return ValidationFailedResult<TokenResponse>.Fail("Invalid token");
        }
    }

    private async Task<List<Claim>> GetClaimsAsync(AppUser appUser)
    {
        List<Claim> claims =
        [
            new(ApplicationClaimTypes.UserId, appUser.Id.ToString()),
            new(ApplicationClaimTypes.LoginName, appUser.LoginName),
            new(ApplicationClaimTypes.FullName, appUser.FullName),
            new(ApplicationClaimTypes.Email, appUser.EmailAddress),
            new(ApplicationClaimTypes.Phone, appUser.PhoneNumber),
        ];
        var permissions = await _userPermissionRepository.GetAllPermissionsAsync(appUser.Id);
        claims.AddRange(permissions.Select(permission =>
            new Claim(ApplicationClaimTypes.Permission, permission.Permission)));
        return claims;
    }

    [LoggerMessage(LogLevel.Error, "Invalid token")]
    private static partial void LogInvalidToken(ILogger logger, Exception e);

    [LoggerMessage(LogLevel.Error, "User not found")]
    private static partial void LogUserNotFound(ILogger logger, Exception e);
}
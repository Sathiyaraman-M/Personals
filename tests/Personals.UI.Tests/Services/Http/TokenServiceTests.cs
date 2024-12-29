using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Personals.Common.Contracts.Tokens;
using Personals.Common.Wrappers;
using Personals.Tests.Base.Fixtures;
using Personals.Tests.Base.Services;
using Personals.UI.Abstractions.Services;
using Personals.UI.Constants;
using Personals.UI.Exceptions;
using Personals.UI.Services.Http;
using System.Globalization;
using System.Security.Claims;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Personals.UI.Tests.Services.Http;

[Collection(nameof(WiremockCollectionFixture))]
public class TokenServiceTests(WiremockFixture wiremockFixture)
{
    private readonly HttpClient _httpClient = wiremockFixture.Server.CreateClient();
    private readonly IPersonalsAuthenticationStateProvider _personalsAuthenticationStateProvider = Substitute.For<IPersonalsAuthenticationStateProvider>(); 
    private readonly ILocalStorageService _localStorageService = Substitute.For<ILocalStorageService>();
    private static StubTimeProvider TimeProvider => new();

    private TokenService TokenService => new(_httpClient, _personalsAuthenticationStateProvider, _localStorageService, TimeProvider);

    [Fact]
    public async Task LoginAsync_ShouldReturnResult()
    {
        // Arrange
        var tokenRequest = new TokenRequest { LoginName = "admin", Password = "admin" };
        var expectedResult = SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
        {
            Token = "token", RefreshToken = "refresh_token", RefreshTokenExpires = DateTime.Now
        });
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        
        // Act
        await TokenService.LoginAsync(tokenRequest);
        
        // Assert
        await _personalsAuthenticationStateProvider.Received(1).UpdateAuthenticationStateAsync(
            expectedResult.Data.Token, expectedResult.Data.RefreshToken, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenResultIsFailed()
    {
        // Arrange
        var tokenRequest = new TokenRequest { LoginName = "admin", Password = "admin" };
        var expectedResult = GenericFailedResult<TokenResponse>.Fail("Login failed");
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/login").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBodyAsJson(expectedResult));
        
        // Act
        var act = async () => await TokenService.LoginAsync(tokenRequest);
        
        // Assert
        await act.Should().ThrowAsync<LoginFailedException>().WithMessage(expectedResult.Messages.FirstOrDefault());
    }
    
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnResult()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns("token");
        _localStorageService.GetItemAsync<string>(StorageConstants.RefreshToken, Arg.Any<CancellationToken>())
            .Returns("refresh_token");
        var expectedResult = SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
        {
            Token = "new_token", RefreshToken = "new_refresh_token", RefreshTokenExpires = DateTime.Now
        });
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/refresh").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        
        // Act
        await TokenService.RefreshTokenAsync();
        
        // Assert
        await _personalsAuthenticationStateProvider.Received(1).UpdateAuthenticationStateAsync(
            expectedResult.Data.Token, expectedResult.Data.RefreshToken, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowException_WhenTokenOrRefreshTokenIsMissing()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(string.Empty);
        _localStorageService.GetItemAsync<string>(StorageConstants.RefreshToken, Arg.Any<CancellationToken>())
            .Returns(string.Empty);
        
        // Act
        var act = async () => await TokenService.RefreshTokenAsync();
        
        // Assert
        await act.Should().ThrowAsync<RefreshTokenFailedException>().WithMessage("Session not found");
    }
    
    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowException_WhenResultIsFailed()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns("token");
        _localStorageService.GetItemAsync<string>(StorageConstants.RefreshToken, Arg.Any<CancellationToken>())
            .Returns("refresh_token");
        var expectedResult = GenericFailedResult<TokenResponse>.Fail("Refresh token failed");
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/refresh").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(400).WithBodyAsJson(expectedResult));
        
        // Act
        var act = async () => await TokenService.RefreshTokenAsync();
        
        // Assert
        await act.Should().ThrowAsync<RefreshTokenFailedException>().WithMessage(expectedResult.Messages.FirstOrDefault());
    }
    
    [Fact]
    public async Task TryRefreshTokenAsync_ShouldRefreshToken_WhenTokenIsExpired()
    {
        // Arrange
        _personalsAuthenticationStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("exp", GetUnixTime(TimeProvider.Now.AddMinutes(-1)))
            ]))));
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns("token");
        _localStorageService.GetItemAsync<string>(StorageConstants.RefreshToken, Arg.Any<CancellationToken>())
            .Returns("refresh_token");
        var expectedResult = SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
        {
            Token = "new_token", 
            RefreshToken = "new_refresh_token", 
            RefreshTokenExpires = DateTime.UtcNow
        });
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/refresh").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        
        // Act
        await TokenService.TryRefreshTokenAsync();
        
        // Assert
        await _personalsAuthenticationStateProvider.Received(1).UpdateAuthenticationStateAsync(
            expectedResult.Data.Token, expectedResult.Data.RefreshToken, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task TryRefreshTokenAsync_ShouldNotRefreshToken_WhenTokenIsNotExpired()
    {
        // Arrange
        _personalsAuthenticationStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("exp", GetUnixTime(TimeProvider.Now.AddMinutes(2)))
            ]))));
        
        // Act
        await TokenService.TryRefreshTokenAsync();
        
        // Assert
        await _personalsAuthenticationStateProvider.DidNotReceive().UpdateAuthenticationStateAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task TryRefreshTokenAsync_ShouldRefreshToken_WhenExpClaimIsMissing()
    {
        // Arrange
        _personalsAuthenticationStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns("token");
        _localStorageService.GetItemAsync<string>(StorageConstants.RefreshToken, Arg.Any<CancellationToken>())
            .Returns("refresh_token");
        var expectedResult = SuccessfulResult<TokenResponse>.Succeed(new TokenResponse
        {
            Token = "new_token", 
            RefreshToken = "new_refresh_token", 
            RefreshTokenExpires = DateTime.UtcNow
        });
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/api/token/refresh").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(expectedResult));
        
        // Act
        await TokenService.TryRefreshTokenAsync();
        
        // Assert
        await _personalsAuthenticationStateProvider.Received(1).UpdateAuthenticationStateAsync(
            expectedResult.Data.Token, expectedResult.Data.RefreshToken, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task LogoutAsync_ShouldRemoveAuthenticationState()
    {
        // Act
        await TokenService.LogoutAsync();
        
        // Assert
        await _personalsAuthenticationStateProvider.Received(1).UpdateAuthenticationStateAsync(
            string.Empty, string.Empty, Arg.Any<CancellationToken>());
    }

    private static string GetUnixTime(DateTime dateTime) => new DateTimeOffset(dateTime)
        .ToUnixTimeSeconds()
        .ToString("0", CultureInfo.InvariantCulture);
}
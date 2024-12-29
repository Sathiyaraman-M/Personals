using Blazored.LocalStorage;
using Personals.Common.Constants;
using Personals.Tests.Base;
using Personals.Tests.Base.Fixtures;
using Personals.UI.Constants;
using Personals.UI.Services;
using System.Net.Http.Headers;

namespace Personals.UI.Tests.Services;

[Collection(nameof(WiremockCollectionFixture))]
public class PersonalsAuthenticationStateProviderTests(WiremockFixture wiremockFixture)
{
    private readonly ILocalStorageService _localStorageService = Substitute.For<ILocalStorageService>();
    private readonly HttpClient _httpClient = wiremockFixture.Server.CreateClient();

    private PersonalsAuthenticationStateProvider PersonalsAuthenticationStateProvider =>
        new(_httpClient, _localStorageService);

    private static class TestUserConstants
    {
        public static readonly Guid DefaultUserId = Guid.NewGuid();
        public const string LoginName = "test";
        public const string FullName = "Test User";
        public const string Email = "test@example.com";
        public const string Phone = "1234567890";
    }

    private static string ValidTestJwtToken => TestJwtBearerBuilder
        .Create()
        .WithUserId(TestUserConstants.DefaultUserId.ToString())
        .WithLoginName(TestUserConstants.LoginName)
        .WithFullName(TestUserConstants.FullName)
        .WithEmail(TestUserConstants.Email)
        .WithPhone(TestUserConstants.Phone)
        .WithPermission(Permissions.Users.View)
        .Build();

    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnEmptyAuthenticationState_WhenJwtTokenIsNotPresent()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        var authenticationState = await PersonalsAuthenticationStateProvider.GetAuthenticationStateAsync();

        // Assert
        authenticationState.User.Identity!.IsAuthenticated.Should().BeFalse();
        authenticationState.User.Claims.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ShouldReturnAuthenticationState_WhenJwtTokenIsPresent()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(ValidTestJwtToken);

        // Act
        var authenticationState = await PersonalsAuthenticationStateProvider.GetAuthenticationStateAsync();

        // Assert
        authenticationState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authenticationState.User.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAuthenticationStateAsync_ShouldRemoveAuthenticationState_WhenJwtTokenIsNotPresent()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(string.Empty);

        // Act
        await PersonalsAuthenticationStateProvider.UpdateAuthenticationStateAsync(string.Empty, string.Empty);

        // Assert
        var authenticationState = await PersonalsAuthenticationStateProvider.GetAuthenticationStateAsync();
        authenticationState.User.Identity!.IsAuthenticated.Should().BeFalse();
        authenticationState.User.Claims.Should().BeEmpty();
        
        _httpClient.DefaultRequestHeaders.Authorization.Should().BeNull();
    }
    
    [Fact]
    public async Task UpdateAuthenticationStateAsync_ShouldUpdateAuthenticationState_WhenJwtTokenIsPresent()
    {
        // Arrange
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(ValidTestJwtToken);

        // Act
        await PersonalsAuthenticationStateProvider.UpdateAuthenticationStateAsync(ValidTestJwtToken, "refreshToken");

        // Assert
        var authenticationState = await PersonalsAuthenticationStateProvider.GetAuthenticationStateAsync();
        authenticationState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authenticationState.User.Claims.Should().NotBeEmpty();
        
        _httpClient.DefaultRequestHeaders.Authorization.Should().NotBeNull().And.Match<AuthenticationHeaderValue>(x =>
            x.Scheme == AuthConstants.JwtBearerScheme && x.Parameter == ValidTestJwtToken);
    }
}
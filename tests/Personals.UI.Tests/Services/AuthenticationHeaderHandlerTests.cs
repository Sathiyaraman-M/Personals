using Blazored.LocalStorage;
using Personals.Tests.Base.Fixtures;
using Personals.UI.Constants;
using Personals.UI.Services;
using System.Net;
using System.Net.Http.Headers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Personals.UI.Tests.Services;

[Collection(nameof(WiremockCollectionFixture))]
public class AuthenticationHeaderHandlerTests(WiremockFixture wiremockFixture)
{
    private readonly ILocalStorageService _localStorageService = Substitute.For<ILocalStorageService>();
    private AuthenticationHeaderHandler AuthenticationHeaderHandler => new(_localStorageService);

    [Fact]
    public async Task SendAsync_ShouldAddAuthorizationHeader_WhenUserIsAuthenticated()
    {
        // Arrange
        const string jwtToken = "jwtToken";
        wiremockFixture.Server
            .Given(Request.Create()
                .WithPath("/")
                .WithHeader(HttpRequestHeader.Authorization.ToString(),
                    new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, jwtToken).ToString())
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200));
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(jwtToken);
        var request = new HttpRequestMessage(HttpMethod.Get, wiremockFixture.Server.Url);
        var httpClient = wiremockFixture.Server.CreateClient(AuthenticationHeaderHandler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.RequestMessage!.Headers.Authorization.Should().NotBeNull();
        response.RequestMessage.Headers.Authorization!.Scheme.Should().Be(AuthConstants.JwtBearerScheme);
        response.RequestMessage.Headers.Authorization.Parameter.Should().Be(jwtToken);

        await _localStorageService.Received(1)
            .GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task
        SendAsync_ShouldNotAddAuthorizationHeader_WhenUserIsAuthenticated_ButRequestAlreadyHasAuthorizationHeader()
    {
        // Arrange
        const string jwtToken = "jwtToken";
        wiremockFixture.Server
            .Given(Request.Create()
                .WithPath("/")
                .WithHeader(HttpRequestHeader.Authorization.ToString(),
                    new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, jwtToken).ToString())
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200));
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(jwtToken);
        var request = new HttpRequestMessage(HttpMethod.Get, wiremockFixture.Server.Url);
        request.Headers.Authorization = new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, jwtToken);
        var httpClient = wiremockFixture.Server.CreateClient(AuthenticationHeaderHandler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.RequestMessage!.Headers.Authorization.Should().NotBeNull();
        response.RequestMessage.Headers.Authorization!.Scheme.Should().Be(AuthConstants.JwtBearerScheme);
        response.RequestMessage.Headers.Authorization.Parameter.Should().Be(jwtToken);

        await _localStorageService.DidNotReceive()
            .GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task SendAsync_ShouldNotAddAuthorizationHeader_WhenUserIsNotAuthenticated()
    {
        // Arrange
        wiremockFixture.Server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200));
        _localStorageService.GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>())
            .Returns(string.Empty);
        var request = new HttpRequestMessage(HttpMethod.Get, wiremockFixture.Server.Url);
        var httpClient = wiremockFixture.Server.CreateClient(AuthenticationHeaderHandler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        response.RequestMessage!.Headers.Authorization.Should().BeNull();

        await _localStorageService.Received(1)
            .GetItemAsync<string>(StorageConstants.AuthToken, Arg.Any<CancellationToken>());
    }
}
using Blazored.LocalStorage;
using Personals.Common.Abstractions.Services;
using Personals.Common.Contracts.Tokens;
using Personals.UI.Abstractions.Services;
using Personals.UI.Extensions;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Constants;
using Personals.UI.Exceptions;
using System.Globalization;
using System.Net.Http.Json;

namespace Personals.UI.Services.Http;

public class TokenService(
    HttpClient httpClient,
    IPersonalsAuthenticationStateProvider authenticationStateProvider,
    ILocalStorageService storageService,
    ITimeProvider timeProvider) : ITokenService
{
    public async Task LoginAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/token/login", tokenRequest, cancellationToken);
        var result = await response.ToResult<TokenResponse>(cancellationToken);
        if (!result.Succeeded)
        {
            throw new LoginFailedException(result.Messages.FirstOrDefault());
        }

        var tokenResponse = result.Data;
        await authenticationStateProvider.UpdateAuthenticationStateAsync(tokenResponse.Token,
            tokenResponse.RefreshToken, cancellationToken);
    }

    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var authToken = await storageService.GetItemAsync<string>(StorageConstants.AuthToken, cancellationToken);
        var refreshToken = await storageService.GetItemAsync<string>(StorageConstants.RefreshToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new RefreshTokenFailedException("Session not found");
        }

        var response = await httpClient.PostAsJsonAsync("api/token/refresh",
            new RefreshTokenRequest { Token = authToken, RefreshToken = refreshToken }, cancellationToken);
        var result = await response.ToResult<TokenResponse>(cancellationToken);
        if (!result.Succeeded)
        {
            throw new RefreshTokenFailedException(result.Messages.FirstOrDefault());
        }

        var tokenResponse = result.Data;
        await authenticationStateProvider.UpdateAuthenticationStateAsync(tokenResponse.Token,
            tokenResponse.RefreshToken, cancellationToken);
    }

    public async Task TryRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var claimsPrincipal = authState.User;
        var expClaim = claimsPrincipal.FindFirst(c => c.Type == "exp");
        var expiringAt =
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim?.Value ?? "0", CultureInfo.InvariantCulture));
        var diff = expiringAt - new DateTimeOffset(timeProvider.Now);
        if (diff.TotalMinutes > 1)
        {
            return;
        }

        await RefreshTokenAsync(cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await authenticationStateProvider.UpdateAuthenticationStateAsync(string.Empty, string.Empty, cancellationToken);
    }
}
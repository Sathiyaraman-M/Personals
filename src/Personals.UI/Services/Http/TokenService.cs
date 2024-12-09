using Blazored.LocalStorage;
using Personals.Common.Contracts.Tokens;
using Personals.UI.Extensions;
using Personals.UI.Abstractions.Services.Http;
using Personals.UI.Constants;
using Personals.UI.Exceptions;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Personals.UI.Services.Http;

public class TokenService(HttpClient httpClient, PayFlowAuthenticationStateProvider authenticationStateProvider, ILocalStorageService storageService) : ITokenService
{
    public async Task LoginAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/token/login", tokenRequest, cancellationToken);
        var result = await response.ToResult<TokenResponse>(cancellationToken);
        if (!result.Succeeded)
        {
            throw new LoginFailedException(result.Messages.FirstOrDefault());
        }
        await UpdateAuthenticationStatusAsync(result.Data, cancellationToken);
    }

    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var authToken = await storageService.GetItemAsync<string>(StorageConstants.AuthToken, cancellationToken);
        var refreshToken = await storageService.GetItemAsync<string>(StorageConstants.RefreshToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }
        var response = await httpClient.PostAsJsonAsync("api/token/refresh", new RefreshTokenRequest { Token = authToken, RefreshToken = refreshToken }, cancellationToken);
        var result = await response.ToResult<TokenResponse>(cancellationToken);
        if (!result.Succeeded)
        {
            throw new RefreshTokenFailedException(result.Messages.FirstOrDefault());
        }
        await UpdateAuthenticationStatusAsync(result.Data, cancellationToken);
    }

    public async Task TryRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var authToken = await storageService.GetItemAsync<string>(StorageConstants.AuthToken, cancellationToken);
        if (string.IsNullOrWhiteSpace(authToken))
        {
            return;
        }
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var claimsPrincipal = authState.User;
        var expClaim = claimsPrincipal.FindFirst(c => c.Type == "exp");
        var expiringAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim?.Value ?? "0", CultureInfo.InvariantCulture));
        var diff = expiringAt - DateTimeOffset.UtcNow;
        if (diff.TotalMinutes > 1)
        {
            return;
        }
        await RefreshTokenAsync(cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await storageService.RemoveItemAsync(StorageConstants.AuthToken, cancellationToken);
        await storageService.RemoveItemAsync(StorageConstants.RefreshToken, cancellationToken);
        authenticationStateProvider.UpdateAuthenticationState(string.Empty);
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    private async Task UpdateAuthenticationStatusAsync(TokenResponse tokenResponse, CancellationToken cancellationToken = default)
    {
        await storageService.SetItemAsync(StorageConstants.AuthToken, tokenResponse.Token, cancellationToken);
        await storageService.SetItemAsync(StorageConstants.RefreshToken, tokenResponse.RefreshToken, cancellationToken);
        authenticationStateProvider.UpdateAuthenticationState(tokenResponse.Token);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthConstants.JwtBearerScheme, tokenResponse.Token);
    }
}
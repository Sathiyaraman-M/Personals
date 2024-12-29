using Microsoft.AspNetCore.Components.Authorization;

namespace Personals.UI.Abstractions.Services;

public interface IPersonalsAuthenticationStateProvider
{
    Task UpdateAuthenticationStateAsync(string jwtToken, string refreshToken,
        CancellationToken cancellationToken = default);
    
    Task<AuthenticationState> GetAuthenticationStateAsync();
}
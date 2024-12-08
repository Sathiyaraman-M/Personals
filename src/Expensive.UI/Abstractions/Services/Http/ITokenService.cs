using Expensive.Common.Contracts.Tokens;

namespace Expensive.UI.Abstractions.Services.Http;

public interface ITokenService
{
    Task LoginAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default);
    
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
    
    Task TryRefreshTokenAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
using Personals.Common.Contracts.Tokens;

namespace Personals.UI.Abstractions.Services.Http;

public interface ITokenService
{
    Task LoginAsync(TokenRequest tokenRequest, CancellationToken cancellationToken = default);
    
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
    
    Task TryRefreshTokenAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
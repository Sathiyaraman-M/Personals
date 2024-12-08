using Expensive.Common.Contracts.Tokens;
using Expensive.Common.Wrappers.Abstractions;

namespace Expensive.Users.Abstractions.Services;

public interface ITokenService
{
    Task<IResult<TokenResponse>> LoginAsync(TokenRequest tokenRequest);

    Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
}
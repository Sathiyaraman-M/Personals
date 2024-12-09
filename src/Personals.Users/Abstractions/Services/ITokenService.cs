using Personals.Common.Contracts.Tokens;
using Personals.Common.Wrappers.Abstractions;

namespace Personals.Users.Abstractions.Services;

public interface ITokenService
{
    Task<IResult<TokenResponse>> LoginAsync(TokenRequest tokenRequest);

    Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
}
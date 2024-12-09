using System.Security.Claims;

namespace Personals.Users.Abstractions.Utilities;

public interface IJwtTokenHandler
{
    string BuildToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ExtractClaimsPrincipal(string token);
}
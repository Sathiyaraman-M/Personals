using Microsoft.Extensions.Configuration;
using Personals.Users.Abstractions.Utilities;

namespace Personals.Users.Utilities;

public class JwtTokenConfiguration(IConfiguration configuration) : IJwtTokenConfiguration
{
    public string Secret =>
        configuration["JWT_SECRET"] ?? throw new ArgumentException("JWT_SECRET is not set properly");

    public string Issuer => configuration["JWT_ISSUER"] ?? "PayFlow";

    public string Audience => configuration["JWT_AUDIENCE"] ?? "PayFlow";

    public int TokenExpirationInMinutes =>
        int.TryParse(configuration["JWT_TOKEN_EXPIRATION_IN_MINUTES"], out var tokenExpirationInMinutes)
            ? tokenExpirationInMinutes
            : 60;

    public int RefreshTokenExpirationInMinutes =>
        int.TryParse(configuration["JWT_REFRESH_TOKEN_EXPIRATION_IN_MINUTES"], out var refreshTokenExpirationInMinutes)
            ? refreshTokenExpirationInMinutes
            : 1440;
}
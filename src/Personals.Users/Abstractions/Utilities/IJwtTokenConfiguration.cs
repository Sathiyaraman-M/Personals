namespace Personals.Users.Abstractions.Utilities;

public interface IJwtTokenConfiguration
{
    string Secret { get; }
    string Issuer { get; }
    string Audience { get; }
    int TokenExpirationInMinutes { get; }
    int RefreshTokenExpirationInMinutes { get; }
}
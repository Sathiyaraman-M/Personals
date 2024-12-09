namespace Personals.Common.Contracts.Tokens;

public class TokenResponse
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime RefreshTokenExpires { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Expensive.Common.Contracts.Tokens;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = null!;
    
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = null!;
}
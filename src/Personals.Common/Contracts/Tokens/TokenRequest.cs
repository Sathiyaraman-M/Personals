using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Tokens;

public class TokenRequest
{
    [Required(ErrorMessage = "Please provide a login name")]
    public string LoginName { get; set; } = null!;
    
    [Required(ErrorMessage = "Please provide a password")]
    public string Password { get; set; } = null!;
}
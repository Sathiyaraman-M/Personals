using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Users;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Please provide a code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Please provide a login name")]
    public string LoginName { get; set; } = null!;

    [Required(ErrorMessage = "Please provide a full name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Please provide a Email address")]
    [EmailAddress(ErrorMessage = "Please provide a valid Email address")]
    public string EmailAddress { get; set; } = null!;

    [Required(ErrorMessage = "Please provide a phone number")]
    [Phone(ErrorMessage = "Please provide a valid phone number")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Please provide a password")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    public string ConfirmPassword { get; set; } = null!;
    
    public bool IsActive { get; set; }
}
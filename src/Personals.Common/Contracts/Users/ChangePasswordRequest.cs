using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Users;

public record ChangePasswordRequest
{
    [Required(ErrorMessage = "Please enter your current password.")] 
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your new password.")] 
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
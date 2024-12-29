using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Users;

public class UpdateUserRequest
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

    public bool IsActive { get; set; }
}
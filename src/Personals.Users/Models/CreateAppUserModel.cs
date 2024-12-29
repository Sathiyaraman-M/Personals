namespace Personals.Users.Models;

public class CreateAppUserModel
{
    public string Code { get; set; } = null!;

    public string LoginName { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; }

    public string CreatedByUserName { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
}
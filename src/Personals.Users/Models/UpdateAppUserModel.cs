namespace Personals.Users.Models;

public class UpdateAppUserModel
{
    public string Code { get; set; } = null!;

    public string LoginName { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public bool IsActive { get; set; }

    public string LastModifiedByUserName { get; set; } = null!;
    public Guid LastModifiedByUserId { get; set; }
}
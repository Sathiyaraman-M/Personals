namespace Expensive.Users.Models;

public class UpdateAppUserModel
{
    public string Code { get; set; } = null!;

    public string LoginName { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string City { get; set; } = null!;
    public string PostCode { get; set; } = null!;
    public string StateCode { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public bool IsActive { get; set; }

    public string LastModifiedByUserName { get; set; } = null!;
    public Guid LastModifiedByUserId { get; set; }
}
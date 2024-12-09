namespace Personals.Common.Contracts.Users;

public record UserResponse(
    Guid Id,
    string Code,
    string LoginName,
    string FullName,
    string Address1,
    string? Address2,
    string City,
    string PostCode,
    string StateCode,
    string EmailAddress,
    string PhoneNumber,
    bool IsActive,
    string CreatedByUserName,
    Guid CreatedByUserId,
    DateTime CreatedOnDate,
    string? LastModifiedByUserName,
    Guid? LastModifiedByUserId,
    DateTime? LastModifiedOnDate)
{
    public int SerialNo { get; set; }
}
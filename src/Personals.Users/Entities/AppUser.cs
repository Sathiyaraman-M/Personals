using Personals.Infrastructure.Abstractions.Entities;

namespace Personals.Users.Entities;

public record AppUser : AuditableEntity
{
    public Guid Id { get; set; }

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

    public string PasswordHash { get; set; } = null!;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    public bool IsActive { get; set; }
}
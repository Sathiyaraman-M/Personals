namespace Personals.Infrastructure.Abstractions.Entities;

public interface IAuditableEntity : IEntity
{
    string CreatedByUserName { get; }
    Guid CreatedByUserId { get; }
    DateTime CreatedOnDate { get; }

    string? LastModifiedByUserName { get; }
    Guid? LastModifiedByUserId { get; }
    DateTime? LastModifiedOnDate { get; }
}

public abstract record AuditableEntity : Entity, IAuditableEntity
{
    public string CreatedByUserName { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOnDate { get; set; }

    public string? LastModifiedByUserName { get; set; }
    public Guid? LastModifiedByUserId { get; set; }
    public DateTime? LastModifiedOnDate { get; set; }
}
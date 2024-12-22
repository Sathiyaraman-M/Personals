namespace Personals.Infrastructure.Abstractions.Entities;

public interface IUserSpecificEntity : IAuditableDateEntity
{
    public Guid UserId { get; }
}
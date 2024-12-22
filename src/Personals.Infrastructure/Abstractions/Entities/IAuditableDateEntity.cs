namespace Personals.Infrastructure.Abstractions.Entities;

public interface IAuditableDateEntity : IEntity
{
    DateTime CreatedOnDate { get; }
    
    DateTime? LastModifiedOnDate { get; }
}
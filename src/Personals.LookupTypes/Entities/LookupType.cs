using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Entities;

namespace Personals.LookupTypes.Entities;

public record LookupType : AuditableEntity
{
    public Guid Id { get; set; }

    public LookupTypeCategory Category { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
}
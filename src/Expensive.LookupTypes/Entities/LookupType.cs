using Expensive.Common.Enums;
using Expensive.Infrastructure.Abstractions.Entities;

namespace Expensive.LookupTypes.Entities;

public record LookupType : AuditableEntity
{
    public Guid Id { get; set; }

    public LookupTypeCategory Category { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
}
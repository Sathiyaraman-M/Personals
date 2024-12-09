using Personals.Common.Enums;

namespace Personals.Common.Contracts.LookupTypes;

public record LookupTypeResponse(
    Guid Id,
    LookupTypeCategory Category,
    string Code,
    string Name,
    string CreatedByUserName,
    DateTime CreatedOnDate,
    string? LastModifiedByUserName,
    DateTime? LastModifiedOnDate)
{
    public int SerialNo { get; set; }
}
using Personals.Common.Enums;

namespace Personals.LookupTypes.Models;

public class UpdateLookupTypeModel
{
    public LookupTypeCategory Category { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string LastModifiedByUserName { get; set; } = null!;

    public Guid LastModifiedByUserId { get; set; }
}
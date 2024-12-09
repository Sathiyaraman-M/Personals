using Personals.Common.Enums;

namespace Personals.LookupTypes.Models;

public class CreateLookupTypeModel
{
    public LookupTypeCategory Category { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string CreatedByUserName { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }
}
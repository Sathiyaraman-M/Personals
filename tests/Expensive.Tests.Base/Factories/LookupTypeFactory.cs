using Expensive.Common.Enums;
using Expensive.LookupTypes.Entities;
using Expensive.Tests.Base.Services;

namespace Expensive.Tests.Base.Factories;

public static class LookupTypeFactory
{
    private static readonly StubTimeProvider TimeProvider = new();

    public static LookupType Create(Guid id, LookupTypeCategory category, string code = "CODE",
        string name = "Look-up Type")
    {
        return new LookupType
        {
            Id = id,
            Category = category,
            Code = code,
            Name = name,
            CreatedByUserName = "Test User",
            CreatedByUserId = Guid.NewGuid(),
            CreatedOnDate = TimeProvider.Now,
        };
    }
}
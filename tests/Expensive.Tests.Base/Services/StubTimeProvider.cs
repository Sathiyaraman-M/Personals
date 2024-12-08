using Expensive.Infrastructure.Abstractions.Services;

namespace Expensive.Tests.Base.Services;

public class StubTimeProvider : ITimeProvider
{
    public DateTime Now => new DateTime(2021, 1, 1).ToUniversalTime();
}
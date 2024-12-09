using Personals.Infrastructure.Abstractions.Services;

namespace Personals.Tests.Base.Services;

public class StubTimeProvider : ITimeProvider
{
    public DateTime Now => new DateTime(2021, 1, 1).ToUniversalTime();
}
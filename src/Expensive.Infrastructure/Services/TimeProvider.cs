using Expensive.Infrastructure.Abstractions.Services;
using System.Diagnostics.CodeAnalysis;

namespace Expensive.Infrastructure.Services;

public class TimeProvider : ITimeProvider
{
    [ExcludeFromCodeCoverage] public DateTime Now => DateTime.UtcNow;
}
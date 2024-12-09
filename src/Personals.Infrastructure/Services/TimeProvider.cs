using Personals.Infrastructure.Abstractions.Services;
using System.Diagnostics.CodeAnalysis;

namespace Personals.Infrastructure.Services;

public class TimeProvider : ITimeProvider
{
    [ExcludeFromCodeCoverage] public DateTime Now => DateTime.UtcNow;
}
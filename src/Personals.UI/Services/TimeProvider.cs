using Personals.Common.Abstractions.Services;
using System.Diagnostics.CodeAnalysis;

namespace Personals.UI.Services;

public class TimeProvider : ITimeProvider
{
    [ExcludeFromCodeCoverage]
    public DateTime Now => DateTime.UtcNow;
}
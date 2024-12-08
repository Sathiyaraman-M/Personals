namespace Expensive.Infrastructure.Abstractions.Services;

public interface ITimeProvider
{
    DateTime Now { get; }
}
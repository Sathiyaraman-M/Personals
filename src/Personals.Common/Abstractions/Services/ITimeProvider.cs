namespace Personals.Common.Abstractions.Services;

public interface ITimeProvider
{
    DateTime Now { get; }
}
namespace Personals.Infrastructure.Abstractions.Services;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    string UserName { get; }

    bool IsAdmin { get; }

    List<KeyValuePair<string, string>> Claims { get; }
}
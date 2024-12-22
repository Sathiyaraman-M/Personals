using Personals.Links.Entities;
using Personals.Tests.Base.Services;

namespace Personals.Tests.Base.Factories;

public static class LinkFactory
{
    private static readonly StubTimeProvider TimeProvider = new();
    
    public static Link Create(Guid id,
        Guid userId,
        string url = "https://www.google.com",
        string? title = "Google",
        string? description = "Search Engine",
        List<string>? tags = null)
    {
        return new Link
        {
            Id = id,
            UserId = userId,
            Url = url,
            Title = title,
            Description = description,
            Tags = tags != null ? string.Join(',', tags) : "",
            CreatedOnDate = TimeProvider.Now,
        };
    }
}
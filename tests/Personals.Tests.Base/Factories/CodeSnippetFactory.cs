using Personals.CodeSnippets.Entities;
using Personals.Common.Enums;
using Personals.Tests.Base.Services;

namespace Personals.Tests.Base.Factories;

public static class CodeSnippetFactory
{
    private static readonly StubTimeProvider TimeProvider = new();
    
    public static CodeSnippet Create(Guid id,
        Guid userId,
        string snippet = "Console.WriteLine(\"Hello, World!\");",
        Language language = Language.CSharp,
        string? title = "Hello World",
        string? remarks = "Prints Hello World",
        List<string>? tags = null)
    {
        return new CodeSnippet
        {
            Id = id,
            UserId = userId,
            Snippet = snippet,
            Language = language,
            Title = title,
            Remarks = remarks,
            Tags = tags != null ? string.Join(',', tags) : "",
            CreatedOnDate = TimeProvider.Now,
        };
    }
}
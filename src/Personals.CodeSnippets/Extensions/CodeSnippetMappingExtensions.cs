using Personals.CodeSnippets.Entities;
using Personals.CodeSnippets.Models;
using Personals.Common.Contracts.CodeSnippets;

namespace Personals.CodeSnippets.Extensions;

public static class CodeSnippetMappingExtensions
{
    public static CodeSnippetModel ToModel(this CodeSnippet codeSnippet)
    {
        return new CodeSnippetModel
        {
            Id = codeSnippet.Id,
            Snippet = codeSnippet.Snippet,
            Language = codeSnippet.Language,
            Title = codeSnippet.Title,
            Remarks = codeSnippet.Remarks,
            Tags = [.. codeSnippet.Tags.Split(',')],
            CreatedOnDate = codeSnippet.CreatedOnDate,
            LastModifiedOnDate = codeSnippet.LastModifiedOnDate
        };
    }
    
    public static CodeSnippet ToCodeSnippet(this CreateCodeSnippetModel model, Guid userId, DateTime createdOnDate)
    {
        return new CodeSnippet
        {
            Id = Guid.NewGuid(),
            Snippet = model.Snippet,
            Language = model.Language,
            Title = model.Title,
            Remarks = model.Remarks,
            Tags = string.Join(',', model.Tags),
            UserId = userId,
            CreatedOnDate = createdOnDate
        };
    }
    
    public static CodeSnippet ToCodeSnippet(this UpdateCodeSnippetModel model, DateTime lastModifiedOnDate)
    {
        return new CodeSnippet
        {
            Snippet = model.Snippet,
            Language = model.Language,
            Title = model.Title,
            Remarks = model.Remarks,
            Tags = string.Join(',', model.Tags),
            LastModifiedOnDate = lastModifiedOnDate
        };
    }
    
    public static CodeSnippetResponse ToResponse(this CodeSnippetModel codeSnippet)
    {
        return new CodeSnippetResponse(
            codeSnippet.Id,
            codeSnippet.Snippet,
            codeSnippet.Language,
            codeSnippet.Title,
            codeSnippet.Remarks,
            codeSnippet.Tags,
            codeSnippet.CreatedOnDate,
            codeSnippet.LastModifiedOnDate
        );
    }
    
    public static CreateCodeSnippetModel ToModel(this CreateCodeSnippetRequest codeSnippet)
    {
        return new CreateCodeSnippetModel
        {
            Snippet = codeSnippet.Snippet,
            Language = codeSnippet.Language,
            Title = codeSnippet.Title,
            Remarks = codeSnippet.Remarks,
            Tags = codeSnippet.Tags
        };
    }
    
    public static UpdateCodeSnippetModel ToModel(this UpdateCodeSnippetRequest codeSnippet)
    {
        return new UpdateCodeSnippetModel
        {
            Snippet = codeSnippet.Snippet,
            Language = codeSnippet.Language,
            Title = codeSnippet.Title,
            Remarks = codeSnippet.Remarks,
            Tags = codeSnippet.Tags
        };
    }
}
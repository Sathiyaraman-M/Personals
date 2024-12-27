using Personals.Common.Enums;

namespace Personals.Common.Contracts.CodeSnippets;

public record CodeSnippetResponse(
    Guid Id,
    string Snippet,
    Language Language,
    string? Title,
    string? Remarks,
    List<string> Tags,
    DateTime CreatedOn,
    DateTime? LastModifiedOn)
{
    public int SerialNo { get; set; } 
}
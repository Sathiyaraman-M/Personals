using Personals.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.CodeSnippets;

public record UpdateCodeSnippetRequest
{
    public UpdateCodeSnippetRequest()
    {
    }
    
    public UpdateCodeSnippetRequest(string snippet, string? title, string? remarks, List<string> tags)
    {
        Snippet = snippet;
        Title = title;
        Remarks = remarks;
        Tags = tags;
    }

    [Required(ErrorMessage = "Please provide a snippet")]
    public string Snippet { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please specify the language")]
    public Language Language { get; set; }
    
    public string? Title { get; set; }
    
    public string? Remarks { get; set; }
    
    public List<string> Tags { get; set; } = [];
}
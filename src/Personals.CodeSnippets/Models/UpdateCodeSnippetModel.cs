using Personals.Common.Enums;

namespace Personals.CodeSnippets.Models;

public record UpdateCodeSnippetModel
{
    public string Snippet { get; set; } = string.Empty;
    
    public Language Language { get; set; }
    
    public string? Title { get; set; }
    
    public string? Remarks { get; set; }
    
    public List<string> Tags { get; set; } = [];
}
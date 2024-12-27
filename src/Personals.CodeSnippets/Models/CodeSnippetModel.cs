using Personals.Common.Enums;

namespace Personals.CodeSnippets.Models;

public class CodeSnippetModel
{
    public Guid Id { get; set; }
    
    public string Snippet { get; set; } = string.Empty;
    
    public Language Language { get; set; }
    
    public string? Title { get; set; }
    
    public string? Remarks { get; set; }
    
    public List<string> Tags { get; set; } = [];
    
    public DateTime CreatedOnDate { get; set; }
    
    public DateTime? LastModifiedOnDate { get; set; }
}
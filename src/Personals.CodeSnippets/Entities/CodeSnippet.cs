using Personals.Common.Enums;
using Personals.Infrastructure.Abstractions.Entities;

namespace Personals.CodeSnippets.Entities;

public class CodeSnippet : IUserSpecificEntity
{
    public Guid Id { get; set; }
    
    public string Snippet { get; set; } = string.Empty;
    
    public Language Language { get; set; }
    
    public string? Title { get; set; }
    
    public string? Remarks { get; set; }
    
    public string Tags { get; set; } = string.Empty;
    
    public Guid UserId { get; set; }
    
    public DateTime CreatedOnDate { get; set; }
    
    public DateTime? LastModifiedOnDate { get; set; }
}
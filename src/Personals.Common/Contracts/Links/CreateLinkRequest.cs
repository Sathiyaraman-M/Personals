using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Links;

public record CreateLinkRequest
{
    [Required(ErrorMessage = "Please provide a URL")]
    public required string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = [];
}
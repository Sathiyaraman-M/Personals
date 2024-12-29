using System.ComponentModel.DataAnnotations;

namespace Personals.Common.Contracts.Links;

public record UpdateLinkRequest
{
    [Required(ErrorMessage = "Please provide a URL")]
    public required string Url { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public List<string> Tags { get; set; } = [];
}